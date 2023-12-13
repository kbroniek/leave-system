using Ardalis.GuardClauses;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using Marten;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using LeaveSystem.Linq;
using LeaveSystem.Shared.Date;
using EFExtensions = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequestValidator
{
    private readonly LeaveSystemDbContext dbContext;
    private readonly IDocumentSession documentSession;
    private readonly CurrentDateService currentDateService;

    public CreateLeaveRequestValidator(LeaveSystemDbContext dbContext, IDocumentSession documentSession,
        CurrentDateService currentDateService)
    {
        this.dbContext = dbContext;
        this.documentSession = documentSession;
        this.currentDateService = currentDateService;
    }

    public virtual void BasicValidate(LeaveRequestCreated creatingLeaveRequest, TimeSpan minDuration,
        TimeSpan maxDuration, bool? includeFreeDays)
    {
        Guard.Against.OutOfRange(creatingLeaveRequest.Duration, nameof(creatingLeaveRequest.Duration), minDuration,
            maxDuration);
        if (includeFreeDays != false) return;
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

    public virtual async Task ImpositionValidator(LeaveRequestCreated creatingLeaveRequest)
    {
        var leaveRequestCreatedEvents = await documentSession.Events.QueryRawEventDataOnly<LeaveRequestCreated>()
            .Where(x => x.CreatedBy.Id == creatingLeaveRequest.CreatedBy.Id && ((
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
                throw new ValidationException(
                    "Cannot create a new leave request in this time. The other leave is overlapping with this date.");
            }
        }
    }

    public virtual async Task LimitValidator(LeaveRequestCreated creatingLeaveRequest)
    {
        var connectedLeaveTypeIds = await GetConnectedLeaveTypeIds(creatingLeaveRequest.LeaveTypeId);
        await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
            creatingLeaveRequest.DateTo,
            creatingLeaveRequest.LeaveTypeId,
            creatingLeaveRequest.CreatedBy.Id,
            creatingLeaveRequest.LeaveTypeId,
            creatingLeaveRequest.Duration,
            connectedLeaveTypeIds.nestedLeaveTypeIds);

        if (connectedLeaveTypeIds.baseLeaveTypeId != null)
        {
            var baseLeaveTypeId = connectedLeaveTypeIds.baseLeaveTypeId.Value;
            await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
                creatingLeaveRequest.DateTo,
                baseLeaveTypeId,
                creatingLeaveRequest.CreatedBy.Id,
                baseLeaveTypeId,
                creatingLeaveRequest.Duration,
                Enumerable.Empty<Guid>());
        }
    }

    private async Task CheckLimitForBaseLeave(
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid currentLeaveTypeId,
        string userId,
        Guid leaveTypeId,
        TimeSpan duration,
        IEnumerable<Guid> nestedLeaveTypeIds)
    {
        UserLeaveLimit leaveLimit = await GetLimits(dateFrom,
            dateTo,
            currentLeaveTypeId,
            userId);
        var totalUsed = await GetUsedLeavesDuration(
            dateFrom.GetFirstDayOfYear(),
            dateFrom.GetLastDayOfYear(),
            userId,
            leaveTypeId,
            nestedLeaveTypeIds);
        if (leaveLimit.Limit != null &&
            CalculateRemaningLimit(leaveLimit.Limit.Value, leaveLimit.OverdueLimit, totalUsed + duration) <= TimeSpan.Zero)
        {
            throw new ValidationException("You don't have enough free days for this type of leave");
        }
    }

    private TimeSpan CalculateRemaningLimit(TimeSpan limit, TimeSpan? overdueLimit, TimeSpan usedLimits)
    {
        return limit + (overdueLimit ?? TimeSpan.Zero) - usedLimits;
    }

    private async Task<(Guid? baseLeaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds)> GetConnectedLeaveTypeIds(Guid leaveTypeId)
    {
        var leaveTypes = await EFExtensions.ToListAsync(dbContext.LeaveTypes.Where(l =>
            l.BaseLeaveTypeId == leaveTypeId || l.Id == leaveTypeId));
        if (leaveTypes.Count == 0)
        {
            return (null, null);
        }

        var nestedLeaveTypes = leaveTypes.Where(l => l.BaseLeaveTypeId == leaveTypeId);
        var currentLeaveType = leaveTypes.FirstOrDefault(l => l.Id == leaveTypeId);
        return (currentLeaveType?.BaseLeaveTypeId, nestedLeaveTypes.Select(x => x.Id));
    }

    private async Task<TimeSpan> GetUsedLeavesDuration(
        DateTimeOffset firstDayOfYear,
        DateTimeOffset lastDayOfYear,
        string userId,
        Guid leaveTypeId,
        IEnumerable<Guid> nestedLeaveTypeIds)
    {
        var nestedLeaveTypeIdsExpression = PredicateBuilder
            .Create<LeaveRequestCreated>(x => x.LeaveTypeId == leaveTypeId);
        var leaveTypeIdArray = nestedLeaveTypeIds as Guid[] ?? nestedLeaveTypeIds.ToArray();
        if (leaveTypeIdArray.Any())
        {
            nestedLeaveTypeIdsExpression = nestedLeaveTypeIdsExpression
                .Or(PredicateBuilder.MatchAny<LeaveRequestCreated, Guid>(x => x.LeaveTypeId,
                    leaveTypeIdArray));
        }
        var connectedLeaveRequestsCreatedExpression = PredicateBuilder.Create<LeaveRequestCreated>(x =>
                x.CreatedBy.Id == userId &&
                x.DateFrom >= firstDayOfYear &&
                x.DateTo <= lastDayOfYear)
            .And(nestedLeaveTypeIdsExpression);
        var leaveRequestCreatedEvents = await documentSession.Events.QueryRawEventDataOnly<LeaveRequestCreated>()
            .Where(connectedLeaveRequestsCreatedExpression).ToListAsync();

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
        string userId)
    {
        var limits = await EFExtensions.ToListAsync(dbContext.UserLeaveLimits.Where(l =>
            (l.AssignedToUserId == null || l.AssignedToUserId == userId) &&
            (l.ValidSince == null || l.ValidSince <= dateFrom) &&
            (l.ValidUntil == null || l.ValidUntil >= dateTo) &&
            l.LeaveTypeId == leaveTypeId));
        if (limits == null || limits.Count == 0)
        {
            throw new ValidationException(
                $"Cannot find limits for the leave type id: {leaveTypeId}. Add limits for the user {userId}.");
        }

        if (limits.Count > 1)
        {
            throw new ValidationException(
                $"Two or more limits found which are the same for the leave type id: {leaveTypeId}. User {userId}.");
        }

        return limits.First();
    }

    public virtual void DateValidator(LeaveRequestCreated creatingLeaveRequest)
    {
        var dateFromWithoutTime = creatingLeaveRequest.DateFrom.GetDayWithoutTime();
        var dateToWithoutTime = creatingLeaveRequest.DateTo.GetDayWithoutTime();
        var now = currentDateService.GetWithoutTime();
        var firstDay = now.GetFirstDayOfYear();
        var lastDay = now.GetLastDayOfYear();
        Guard.Against.OutOfRange(dateFromWithoutTime, nameof(creatingLeaveRequest.DateFrom), firstDay, lastDay);
        Guard.Against.OutOfRange(dateToWithoutTime, nameof(creatingLeaveRequest.DateTo), firstDay, lastDay);
        if (dateFromWithoutTime > dateToWithoutTime)
        {
            throw new ArgumentOutOfRangeException(nameof(creatingLeaveRequest.DateFrom),
                "Date from has to be less than date to.");
        }
    }
}