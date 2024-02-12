using System.Runtime.Serialization;
using GoldenEye.Backend.Core.DDD.Events;
using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.DeprecatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.Periods;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using Marten.Events.Aggregation;

namespace LeaveSystem.EventSourcing.LeaveRequests;

public class LeaveRequest : IEventSource, INotNullablePeriod
{
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

    [IgnoreDataMember]
    Queue<IEvent> IEventSource.PendingEvents { get; } = new Queue<IEvent>();

    public Guid Id { get; protected set; }

    object IHaveId.Id => Id;

    //For serialization
    public LeaveRequest() { }

    private LeaveRequest(LeaveRequestCreated @event)
    {
        Append(@event);
        Apply(@event);
    }

    public static LeaveRequest CreatePendingLeaveRequest(LeaveRequestCreated @event) => new(@event);

    internal void Accept(string? remarks, FederatedUser acceptedBy)
    {
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Rejected)
        {
            throw new InvalidOperationException($"Accepting leave request in '{Status}' status is not allowed.");
        }

        var @event = LeaveRequestAccepted.Create(Id, remarks, acceptedBy);

        Append(@event);
        Apply(@event);
    }
    internal void Reject(string? remarks, FederatedUser rejectedBy)
    {
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
        {
            throw new InvalidOperationException($"Rejecting leave request in '{Status}' status is not allowed.");
        }

        var @event = LeaveRequestRejected.Create(Id, remarks, rejectedBy);

        Append(@event);
        Apply(@event);
    }

    internal void Cancel(string? remarks, FederatedUser canceledBy, DateTimeOffset now)
    {
        if (!string.Equals(CreatedBy.Id, canceledBy.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Canceling a non-your leave request is not allowed.");
        }
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
        {
            throw new InvalidOperationException($"Canceling leave requests in '{Status}' status is not allowed.");
        }
        if (DateFrom < now)
        {
            throw new InvalidOperationException("Canceling of past leave requests is not allowed.");
        }

        var @event = LeaveRequestCanceled.Create(Id, remarks, canceledBy);

        Append(@event);
        Apply(@event);
    }

    internal void OnBehalf(FederatedUser createdByOnBehalf)
    {
        if (Status != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Creating on behalf leave request in {Status} status is not allowed. Only {LeaveRequestStatus.Pending} is allowed.");
        }
        var @event = LeaveRequestOnBehalfCreated.Create(Id, createdByOnBehalf);

        Append(@event);
        Apply(@event);
    }

    internal void Deprecate(string? remarks, FederatedUser deprecatedBy)
    {
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
        {
            throw new InvalidOperationException($"Deprecating leave request in '{Status}' status is not allowed.");
        }
        var @event = LeaveRequestDeprecated.Create(Id, remarks, deprecatedBy);
        Append(@event);
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

    private void Apply(LeaveRequestOnBehalfCreated @event)
    {
        CreatedByOnBehalf = @event.CreatedByOnBehalf;
        LastModifiedBy = @event.CreatedByOnBehalf;
        Version++;
    }

    private void Apply(LeaveRequestDeprecated @event)
    {
        LastModifiedBy = @event.DeprecatedBy;
        Status = LeaveRequestStatus.Deprecated;
        AddRemarks(@event.Remarks, @event.DeprecatedBy);
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

    private void Append(IEvent @event) => (this as IEventSource).PendingEvents.Enqueue(@event);
    public class LeaveRequestProjection : SingleStreamProjection<LeaveRequest>
    {
        public LeaveRequestProjection()
        {
            ProjectEvent<LeaveRequestCreated>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestAccepted>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestRejected>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestCanceled>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestDeprecated>((item, @event) => item.Apply(@event));
        }
    }
}
