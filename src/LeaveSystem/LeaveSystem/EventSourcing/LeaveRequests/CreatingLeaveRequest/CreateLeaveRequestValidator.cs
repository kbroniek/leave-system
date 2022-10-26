using Ardalis.GuardClauses;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using Marten;
using System.ComponentModel.DataAnnotations;
using EFExtensions = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
public class CreateLeaveRequestValidator
{
    private readonly LeaveSystemDbContext dbContext;
    private readonly WorkingHoursService workingHoursService;
    private readonly IDocumentSession documentSession;

    public CreateLeaveRequestValidator(LeaveSystemDbContext dbContext, WorkingHoursService workingHoursService, IDocumentSession documentSession)
    {
        this.dbContext = dbContext;
        this.workingHoursService = workingHoursService;
        this.documentSession = documentSession;
    }

    public virtual void BasicValidate(LeaveRequestCreated creatingLeaveRequest, TimeSpan minDuration, TimeSpan maxDuration, bool? includeFreeDays)
    {
        Guard.Against.OutOfRange(creatingLeaveRequest.Duration, nameof(creatingLeaveRequest.Duration), minDuration, maxDuration);
        if (includeFreeDays == false)
        {
            var dateFromDayKind = DateCalculator.GetDayKind(creatingLeaveRequest.DateFrom);
            if (dateFromDayKind != DateCalculator.DayKind.WORKING)
            {
                throw new ArgumentOutOfRangeException(nameof(creatingLeaveRequest.DateFrom), "The date is off work.");
            }
            var dateToDayKind = DateCalculator.GetDayKind(creatingLeaveRequest.DateTo);
            if (dateToDayKind != DateCalculator.DayKind.WORKING)
            {
                throw new ArgumentOutOfRangeException(nameof(creatingLeaveRequest.DateTo), "The date is off work.");
            }
        }
    }

    public virtual async Task ImpositionValidator(LeaveRequestCreated creatingLeaveRequest)
    {
        var leaveRequestCreatedEvents = await documentSession.Events.QueryRawEventDataOnly<LeaveRequestCreated>()
            .Where(x => x.CreatedBy.Email == creatingLeaveRequest.CreatedBy.Email && ((
                    x.DateFrom >= creatingLeaveRequest.DateTo &&
                    x.DateTo <= creatingLeaveRequest.DateTo
                ) || (
                    x.DateFrom >= creatingLeaveRequest.DateFrom &&
                    x.DateTo <= creatingLeaveRequest.DateFrom
                ) || (
                    x.DateFrom >= creatingLeaveRequest.DateFrom &&
                    x.DateTo <= creatingLeaveRequest.DateTo
                ) || (
                    x.DateFrom <= creatingLeaveRequest.DateFrom &&
                    x.DateTo >= creatingLeaveRequest.DateTo
                )))
            .ToListAsync();

        foreach (var @event in leaveRequestCreatedEvents)
        {
            var leaveRequestFromDb = await documentSession.Events.AggregateStreamAsync<LeaveRequest>(@event.StreamId);
            if (leaveRequestFromDb?.Status.IsValid() == true)
            {
                throw new ValidationException("Cannot create a new leave request in this time. The other leave is overlapping with this date.");
            }
        }
    }

    public virtual async Task LimitValidator(LeaveRequestCreated creatingLeaveRequest)
    {
        var connectedLeaveTypeIds = await GetConnectedLeaveTypeIds(creatingLeaveRequest.LeaveTypeId);
        await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
            creatingLeaveRequest.DateTo,
            creatingLeaveRequest.LeaveTypeId,
            creatingLeaveRequest.CreatedBy.Email!,
            creatingLeaveRequest.LeaveTypeId,
            connectedLeaveTypeIds.nestedLeaveTypeId);

        if (connectedLeaveTypeIds.baseLeaveTypeId != null)
        {
            var baseLeaveTypeId = connectedLeaveTypeIds.baseLeaveTypeId.Value;
            await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
                creatingLeaveRequest.DateTo,
                baseLeaveTypeId,
                creatingLeaveRequest.CreatedBy.Email!,
                baseLeaveTypeId);
        }
    }

    private async Task CheckLimitForBaseLeave(
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid currentLeaveTypeId,
        string userEmail,
        Guid leaveTypeId,
        Guid? nestedLeaveTypeId = null)
    {
        UserLeaveLimit leaveLimit = await GetLimits(dateFrom,
               dateTo,
               currentLeaveTypeId,
               userEmail);
        var totalUsed = await GetUsedLeavesDuration(dateFrom,
            userEmail,
            leaveTypeId,
            nestedLeaveTypeId);
        if (leaveLimit.Limit != null && CalculateRemaningLimit(leaveLimit.Limit.Value, leaveLimit.OverdueLimit, totalUsed) <= TimeSpan.Zero)
        {
            throw new ValidationException("You don't have enough free days for this type of leave");
        }
    }

    private TimeSpan CalculateRemaningLimit(TimeSpan limit, TimeSpan? overdueLimit, TimeSpan usedLimits)
    {
        return limit + (overdueLimit ?? TimeSpan.Zero) - usedLimits;
    }

    private async Task<(Guid? baseLeaveTypeId, Guid? nestedLeaveTypeId)> GetConnectedLeaveTypeIds(Guid leaveTypeId)
    {
        var leaveTypes = await EFExtensions.ToListAsync(dbContext.LeaveTypes.Where(l =>
            l.BaseLeaveTypeId == leaveTypeId || l.Id == leaveTypeId));
        if (leaveTypes.Count == 0)
        {
            return (null, null);
        }
        var nestedLeaveType = leaveTypes.FirstOrDefault(l => l.BaseLeaveTypeId == leaveTypeId);
        var currentLeaveType = leaveTypes.FirstOrDefault(l => l.Id == leaveTypeId);
        return (currentLeaveType?.BaseLeaveTypeId, nestedLeaveType?.Id);
    }

    private async Task<TimeSpan> GetUsedLeavesDuration(
        DateTimeOffset dateFrom,
        string userEmail,
        Guid leaveTypeId,
        Guid? nestedLeaveTypeId)
    {
        var firstDay = dateFrom.GetFirstDayOfYear();
        var lastDay = dateFrom.GetLastDayOfYear();
        var leaveRequestCreatedEvents = await documentSession.Events.QueryRawEventDataOnly<LeaveRequestCreated>()
            .Where(x => x.CreatedBy.Email == userEmail &&
                x.DateFrom >= firstDay &&
                x.DateTo <= lastDay &&
                (
                    x.LeaveTypeId == leaveTypeId ||
                    (nestedLeaveTypeId != null && x.LeaveTypeId == nestedLeaveTypeId)
                )
             ).ToListAsync();

        TimeSpan usedLimits = TimeSpan.Zero;
        foreach (var @event in leaveRequestCreatedEvents)
        {
            var leaveRequestFromDb = await documentSession.Events.AggregateStreamAsync<LeaveRequest>(@event.StreamId);
            if (leaveRequestFromDb?.Status.IsValid() == true)
            {
                usedLimits += leaveRequestFromDb.Duration;
            }
        }
        return usedLimits;
    }

    private async Task<UserLeaveLimit> GetLimits(
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid leaveTypeId,
        string userEmail)
    {
        var limits = await EFExtensions.ToListAsync(dbContext.UserLeaveLimits.Where(l =>
                        (l.AssignedToUserEmail == null || l.AssignedToUserEmail == userEmail) &&
                        (l.ValidSince == null || l.ValidSince <= dateFrom) &&
                        (l.ValidUntil == null || l.ValidUntil >= dateTo) &&
                        l.LeaveTypeId == leaveTypeId));
        if (limits == null || limits.Count == 0)
        {
            throw new ValidationException($"Cannot find limits for the leave type id: {leaveTypeId}. Add limits for the user {userEmail}.");
        }
        if (limits.Count > 1)
        {
            throw new ValidationException($"Two or more limits found which are the same for the leave type id: {leaveTypeId}. User {userEmail}.");
        }
        return limits.First();
    }
}
