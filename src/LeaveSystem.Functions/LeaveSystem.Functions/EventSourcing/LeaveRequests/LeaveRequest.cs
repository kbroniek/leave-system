namespace LeaveSystem.Functions.EventSourcing.LeaveRequests;

using LeaveSystem.Functions.EventSourcing.LeaveRequests.Events;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;

internal class LeaveRequest
{
    public Guid Id { get; set; }
    public DateTimeOffset DateFrom { get; private set; }

    public DateTimeOffset DateTo { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public List<RemarksModel> Remarks { get; } = new();

    public LeaveRequestStatus Status { get; private set; }

    public FederatedUser CreatedBy { get; private set; }

    public FederatedUser LastModifiedBy { get; private set; }

    public FederatedUser? CreatedByOnBehalf { get; private set; }

    public TimeSpan WorkingHours { get; private set; }
    public int Version { get; private set; }

    private void Apply(LeaveRequestCreated @event)
    {
        Id = @event.LeaveRequestId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        LeaveTypeId = @event.LeaveTypeId;
        AddRemarks(@event.Remarks, @event.CreatedBy);
        Status = LeaveRequestStatus.Pending;
        CreatedBy = @event.CreatedBy;
        LastModifiedBy = @event.CreatedBy;
        WorkingHours = @event.WorkingHours;
        Version++;
    }

    private void Apply(Event @event)
    {
        switch (@event)
        {
            case LeaveRequestCreated leaveRequestCreated:
                Apply(leaveRequestCreated);
                break;
        }
    }

    private void AddRemarks(string? remarks, FederatedUser createdBy)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return;
        }
        Remarks.Add(new RemarksModel(remarks, createdBy));
    }
}

public record RemarksModel(string Remarks, FederatedUser CreatedBy);
