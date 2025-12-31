namespace LeaveSystem.Domain.LeaveRequests.Canceling;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared.Dto;

public record LeaveRequestCanceled(
    Guid LeaveRequestId,
    string? Remarks,
    LeaveRequestUserDto CanceledBy,
    DateTimeOffset CreatedDate
) : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public static LeaveRequestCanceled Create(Guid leaveRequestId, string? remarks, LeaveRequestUserDto canceledBy, DateTimeOffset createdDate) =>
        new(leaveRequestId, remarks, canceledBy, createdDate);
}
