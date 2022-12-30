using GoldenEye.Aggregates;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;

namespace LeaveSystem.EventSourcing.LeaveRequests;

public class LeaveRequest : Aggregate
{
    public DateTimeOffset DateFrom { get; private set; }

    public DateTimeOffset DateTo { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public List<RemarksModel> Remarks { get; } = new();

    public LeaveRequestStatus Status { get; private set; }

    public FederatedUser CreatedBy { get; private set; }

    public FederatedUser LastModifiedBy { get; private set; }

    public FederatedUser? CreatedByBehalfOn { get; private set; }

    //For serialization
    public LeaveRequest() { }

    private LeaveRequest(LeaveRequestCreated @event)
    {
        Enqueue(@event);
        Apply(@event);
    }

    public static LeaveRequest CreatePendingLeaveRequest(LeaveRequestCreated @event) => new(@event);

    internal void Accept(string? remarks, FederatedUser acceptedBy)
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Accepting leave request in '{Status}' status is not allowed.");
        }

        var @event = LeaveRequestAccepted.Create(Id, remarks, acceptedBy);

        Enqueue(@event);
        Apply(@event);
    }
    internal void Reject(string? remarks, FederatedUser rejectedBy)
    {
        if (Status != LeaveRequestStatus.Pending && Status != LeaveRequestStatus.Accepted)
        {
            throw new InvalidOperationException($"Rejecting leave request in '{Status}' status is not allowed.");
        }

        var @event = LeaveRequestRejected.Create(Id, remarks, rejectedBy);

        Enqueue(@event);
        Apply(@event);
    }

    internal void Cancel(string? remarks, FederatedUser canceledBy)
    {
        if (!string.Equals(CreatedBy.Id, canceledBy.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Canceling a non-your leave request is not allowed.");
        }
        if (Status != LeaveRequestStatus.Pending && Status != LeaveRequestStatus.Accepted)
        {
            throw new InvalidOperationException($"Canceling leave requests in '{Status}' status is not allowed.");
        }
        if (DateFrom < DateTimeOffset.UtcNow)
        {
            //throw new InvalidOperationException($"Canceling of past leave requests is not allowed.");
        }

        var @event = LeaveRequestCanceled.Create(Id, remarks, canceledBy);

        Enqueue(@event);
        Apply(@event);
    }

    internal void BehalfOn(FederatedUser createdByBehalfOn)
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Creating on behalf leave request in {Status} status is not allowed. Only {LeaveRequestStatus.Pending} is allowed.");
        }
        var @event = LeaveRequestBehalfOnCreated.Create(Id, createdByBehalfOn);

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
        LastModifiedBy = @event.CreatedBy;
        Version++;
    }

    private void Apply(LeaveRequestAccepted @event)
    {
        Status = LeaveRequestStatus.Accepted;
        AddRemarks(@event.Remarks, @event.AcceptedBy);
        LastModifiedBy = @event.AcceptedBy;
        Version++;
    }

    private void Apply(LeaveRequestRejected @event)
    {
        Status = LeaveRequestStatus.Rejected;
        AddRemarks(@event.Remarks, @event.RejectedBy);
        LastModifiedBy = @event.RejectedBy;
        Version++;
    }

    private void Apply(LeaveRequestCanceled @event)
    {
        Status = LeaveRequestStatus.Canceled;
        AddRemarks(@event.Remarks, @event.CanceledBy);
        LastModifiedBy = @event.CanceledBy;
        Version++;
    }

    private void Apply(LeaveRequestBehalfOnCreated @event)
    {
        CreatedByBehalfOn = @event.CreatedByBehalfOn;
        LastModifiedBy = @event.CreatedByBehalfOn;
        Version++;
    }

    private void AddRemarks(string? remarks, FederatedUser createdBy)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return;
        }
        Remarks.Add(new RemarksModel(remarks, createdBy));
    }

    public record RemarksModel(string Remarks, FederatedUser CreatedBy);
}
