using LeaveSystem.Shared.LeaveRequests;
using Marten;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class ImpositionValidator
{
    private readonly IDocumentSession documentSession;

    public ImpositionValidator(IDocumentSession documentSession)
    {
        this.documentSession = documentSession;
    }
    public virtual async Task Validate(LeaveRequestCreated creatingLeaveRequest)
    {
        var leaveRequestCreatedEvents = await documentSession.Events.QueryRawEventDataOnly<LeaveRequestCreated>()
            .Where(x => x.CreatedBy.Id == creatingLeaveRequest.CreatedBy.Id && (
                x.DateFrom >= creatingLeaveRequest.DateTo &&
                x.DateTo <= creatingLeaveRequest.DateTo
             ||
                x.DateFrom >= creatingLeaveRequest.DateFrom &&
                x.DateTo <= creatingLeaveRequest.DateFrom
             ||
                x.DateFrom >= creatingLeaveRequest.DateFrom &&
                x.DateTo <= creatingLeaveRequest.DateTo
             ||
                x.DateFrom <= creatingLeaveRequest.DateFrom &&
                x.DateTo >= creatingLeaveRequest.DateTo
            ))
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
}
