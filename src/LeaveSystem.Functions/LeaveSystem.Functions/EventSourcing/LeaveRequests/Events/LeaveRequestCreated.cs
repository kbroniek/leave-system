namespace LeaveSystem.Functions.EventSourcing.LeaveRequests.Events;
using LeaveSystem.Shared;

internal class LeaveRequestCreated : Event
{
    public override Guid StreamId => LeaveRequestId;
    public Guid LeaveRequestId { get; }

    public DateTimeOffset DateFrom { get; }

    public DateTimeOffset DateTo { get; }

    public TimeSpan Duration { get; }

    public Guid LeaveTypeId { get; }

    public string? Remarks { get; }

    public FederatedUser CreatedBy { get; }

    public TimeSpan WorkingHours { get; }
}
