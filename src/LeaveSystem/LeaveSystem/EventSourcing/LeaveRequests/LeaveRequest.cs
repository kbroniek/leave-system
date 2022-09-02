using GoldenEye.Aggregates;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests.ApprovingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

namespace LeaveSystem.EventSourcing.LeaveRequests;

public class LeaveRequest : Aggregate
{
    public DateTimeOffset DateFrom { get; private set; }

    public DateTimeOffset DateTo { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public List<RemarksModel> Remarks { get; private set; } = new ();

    public LeaveRequestStatus Status { get; private set; }

    public FederatedUser CreatedBy { get; private set; }

    public FederatedUser? LastModifiedBy { get; private set; }

    //For serialization
    public LeaveRequest() { }

    private LeaveRequest(LeaveRequestCreated @event)
    {
        Enqueue(@event);
        Apply(@event);
    }

    public static LeaveRequest Create(LeaveRequestCreated @event) => new(@event);

    internal void Approve(string? remarks, FederatedUser approvedBy)
    {
        if (Status != LeaveRequestStatus.Pending)
            throw new InvalidOperationException($"Approving leave request in '{Status}' status is not allowed.");

        var @event = LeaveRequestApproved.Create(Id, remarks, approvedBy);

        Enqueue(@event);
        Apply(@event);
    }

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
        Version++;
    }

    private void Apply(LeaveRequestApproved @event)
    {
        Status = LeaveRequestStatus.Approved;
        AddRemarks(@event.Remarks, @event.ApprovedBy);
        LastModifiedBy = @event.ApprovedBy;
    }

    private void AddRemarks(string? remarks, FederatedUser createdBy)
    {
        if(string.IsNullOrWhiteSpace(remarks))
        {
            return;
        }
        Remarks.Add(new RemarksModel(remarks, createdBy));
    }

    public record RemarksModel(string remarks, FederatedUser createdBy);
}
