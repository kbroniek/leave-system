namespace LeaveSystem.Domain.LeaveRequests.Accepting;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;

public record LeaveRequestAccepted(
    Guid LeaveRequestId,
    string? Remarks,
    FederatedUser AcceptedBy,
    DateTimeOffset CreatedDate
) : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public static LeaveRequestAccepted Create(Guid leaveRequestId, string? remarks, FederatedUser acceptedBy, DateTimeOffset createdDate) =>
        new(leaveRequestId, remarks, acceptedBy, createdDate);
}
