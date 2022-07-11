using Ardalis.GuardClauses;
using Baseline;
using LeaveSystem.Db;
using LeaveSystem.Services;
using Marten;
using System.ComponentModel.DataAnnotations;

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

    public virtual async Task Validate(LeaveRequest leaveRequest, TimeSpan minDuration, TimeSpan maxDuration, bool? includeFreeDays, TimeSpan workingHours)
    {
        BasicValidate(leaveRequest, minDuration, maxDuration, includeFreeDays);
        await ImpositionValidator(leaveRequest);
    }

    private void BasicValidate(LeaveRequest leaveRequest, TimeSpan minDuration, TimeSpan maxDuration, bool? includeFreeDays)
    {
        Guard.Against.OutOfRange(leaveRequest.Duration, nameof(leaveRequest.Duration), minDuration, maxDuration);
        if (includeFreeDays == false)
        {
            var dateFromDayKind = workingHoursService.getDayKind(leaveRequest.DateFrom);
            if (dateFromDayKind != WorkingHoursService.DayKind.WORKING)
            {
                throw new ArgumentOutOfRangeException(nameof(leaveRequest.DateFrom), "The date is off work.");
            }
            var dateToDayKind = workingHoursService.getDayKind(leaveRequest.DateTo);
            if (dateToDayKind != WorkingHoursService.DayKind.WORKING)
            {
                throw new ArgumentOutOfRangeException(nameof(leaveRequest.DateTo), "The date is off work.");
            }
        }
    }

    private async Task ImpositionValidator(LeaveRequest creatingLeaveRequest)
    {
        var leaveRequestCreatedEvents = await documentSession.Events.QueryAllRawEvents()
            .Where(x => x.EventTypeName == "leave_request_created" &&
                x.As<LeaveRequestCreated>().CreatedBy.Email == creatingLeaveRequest.CreatedBy.Email && (!(
                    x.As<LeaveRequestCreated>().DateFrom < creatingLeaveRequest.DateTo
                ) || !(
                    x.As<LeaveRequestCreated>().DateTo > creatingLeaveRequest.DateFrom
                )))
            .ToListAsync();
        var leaveRequestsFromDb = await Task.WhenAll(leaveRequestCreatedEvents
            .Select(e => documentSession.Events.AggregateStreamAsync<LeaveRequest>(e.StreamId)));
        if(leaveRequestsFromDb.Any(l => (l?.Status & LeaveRequestStatus.Valid) > 0))
        {
            throw new ValidationException("Cannot create a new leave request in this time. The other leave is overlapping with this date.");
        }
    }
}
