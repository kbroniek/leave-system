namespace LeaveSystem.Domain.LeaveRequests.Rejecting;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared.Dto;

public record LeaveRequestRejected(
    Guid LeaveRequestId,
    string? Remarks,
    LeaveRequestUserDto RejectedBy,
    DateTimeOffset CreatedDate
) : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public static LeaveRequestRejected Create(Guid leaveRequestId, string? remarks, LeaveRequestUserDto rejectedBy, DateTimeOffset createdDate) =>
        new(leaveRequestId, remarks, rejectedBy, createdDate);
}
