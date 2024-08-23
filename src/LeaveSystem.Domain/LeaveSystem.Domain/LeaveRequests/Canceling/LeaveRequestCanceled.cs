using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;

namespace LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;

public record LeaveRequestCanceled(
    Guid LeaveRequestId,
    string? Remarks,
    FederatedUser CanceledBy,
    DateTimeOffset CreatedDate
) : IEvent
{
    public Guid StreamId => LeaveRequestId;
}
