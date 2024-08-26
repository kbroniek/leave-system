namespace LeaveSystem.Domain.LeaveRequests.Accepting;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared.Dto;

public record LeaveRequestAccepted(
    Guid LeaveRequestId,
    string? Remarks,
    LeaveRequestUserDto AcceptedBy,
    DateTimeOffset CreatedDate
) : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public static LeaveRequestAccepted Create(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate) =>
        new(leaveRequestId, remarks, acceptedBy, createdDate);
}
