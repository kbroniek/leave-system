namespace LeaveSystem.Domain.LeaveRequests;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;

public class LeaveRequest
{
    private readonly List<RemarksModel> remarks = [];

    public Guid Id { get; private set; }

    public DateOnly DateFrom { get; private set; }

    public DateOnly DateTo { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public IReadOnlyCollection<RemarksModel> Remarks => remarks;

    public LeaveRequestStatus Status { get; private set; }

    public FederatedUser CreatedBy { get; private set; }

    public FederatedUser LastModifiedBy { get; private set; }

    public FederatedUser AssignedTo { get; private set; }

    public TimeSpan WorkingHours { get; private set; }

    public int Version { get; private set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }

    //[IgnoreDataMember]
    //queue<ievent> ieventsource.pendingevents { get; } = new queue<ievent>();


    private LeaveRequest() { }

    public static LeaveRequest Default() =>
        new();

    public static LeaveRequest Evolve(LeaveRequest leaveRequest, IEvent @event) =>
        @event switch
        {
            LeaveRequestCreated createdEvent =>
                leaveRequest.Pending(createdEvent),
            LeaveRequestAccepted(var leaveRequestId, var acceptedRemarks, var acceptedBy, var createdDate) =>
                leaveRequest.Accept(leaveRequestId, acceptedRemarks, acceptedBy, createdDate),
            _ => leaveRequest
        };

    internal LeaveRequest Pending(LeaveRequestCreated @event) => Apply(@event);

    internal LeaveRequest Accept(Guid leaveReuestId, string? remarks, FederatedUser acceptedBy, DateTimeOffset createdDate)
    {
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Rejected)
        {
            throw new InvalidOperationException($"Accepting leave request in '{Status}' status is not allowed.");
        }
        if (leaveReuestId != Id)
        {
            throw new InvalidOperationException($"Accepting leave request in different id is not allowed.");
        }

        var @event = LeaveRequestAccepted.Create(Id, remarks, acceptedBy, createdDate);

        //Append(@event);
        Apply(@event);
        return this;
    }
    //internal void Reject(string? remarks, FederatedUser rejectedBy)
    //{
    //    if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
    //    {
    //        throw new InvalidOperationException($"Rejecting leave request in '{Status}' status is not allowed.");
    //    }

    //    var @event = LeaveRequestRejected.Create(Id, remarks, rejectedBy);

    //    Append(@event);
    //    Apply(@event);
    //}

    //internal void Cancel(string? remarks, FederatedUser canceledBy, DateTimeOffset now)
    //{
    //    if (!string.Equals(CreatedBy.Id, canceledBy.Id, StringComparison.OrdinalIgnoreCase))
    //    {
    //        throw new InvalidOperationException("Canceling a non-your leave request is not allowed.");
    //    }
    //    if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
    //    {
    //        throw new InvalidOperationException($"Canceling leave requests in '{Status}' status is not allowed.");
    //    }
    //    if (DateFrom < now)
    //    {
    //        throw new InvalidOperationException("Canceling of past leave requests is not allowed.");
    //    }

    //    var @event = LeaveRequestCanceled.Create(Id, remarks, canceledBy);

    //    Append(@event);
    //    Apply(@event);
    //}

    //internal void OnBehalf(FederatedUser createdByOnBehalf)
    //{
    //    if (Status != LeaveRequestStatus.Pending)
    //    {
    //        throw new InvalidOperationException($"Creating on behalf leave request in {Status} status is not allowed. Only {LeaveRequestStatus.Pending} is allowed.");
    //    }
    //    var @event = LeaveRequestOnBehalfCreated.Create(Id, createdByOnBehalf);

    //    Append(@event);
    //    Apply(@event);
    //}

    //internal void Deprecate(string? remarks, FederatedUser deprecatedBy)
    //{
    //    if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
    //    {
    //        throw new InvalidOperationException($"Deprecating leave request in '{Status}' status is not allowed.");
    //    }
    //    var @event = LeaveRequestDeprecated.Create(Id, remarks, deprecatedBy);
    //    Append(@event);
    //    Apply(@event);
    //}

    private LeaveRequest Apply(LeaveRequestCreated @event)
    {
        Id = @event.LeaveRequestId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        LeaveTypeId = @event.LeaveTypeId;
        AddRemarks(@event.Remarks, @event.CreatedBy, @event.CreatedDate);
        Status = LeaveRequestStatus.Pending;
        CreatedBy = @event.CreatedBy;
        LastModifiedBy = @event.CreatedBy;
        WorkingHours = @event.WorkingHours;
        Version++;
        return this;
    }

    private void Apply(LeaveRequestAccepted @event)
    {
        Status = LeaveRequestStatus.Accepted;
        AddRemarks(@event.Remarks, @event.AcceptedBy, @event.CreatedDate);
        LastModifiedBy = @event.AcceptedBy;
        Version++;
    }

    //private void Apply(LeaveRequestRejected @event)
    //{
    //    Status = LeaveRequestStatus.Rejected;
    //    AddRemarks(@event.Remarks, @event.RejectedBy);
    //    LastModifiedBy = @event.RejectedBy;
    //    Version++;
    //}

    //private void Apply(LeaveRequestCanceled @event)
    //{
    //    Status = LeaveRequestStatus.Canceled;
    //    AddRemarks(@event.Remarks, @event.CanceledBy);
    //    LastModifiedBy = @event.CanceledBy;
    //    Version++;
    //}

    //private void Apply(LeaveRequestOnBehalfCreated @event)
    //{
    //    CreatedByOnBehalf = @event.CreatedByOnBehalf;
    //    LastModifiedBy = @event.CreatedByOnBehalf;
    //    Version++;
    //}

    //private void Apply(LeaveRequestDeprecated @event)
    //{
    //    LastModifiedBy = @event.DeprecatedBy;
    //    Status = LeaveRequestStatus.Deprecated;
    //    AddRemarks(@event.Remarks, @event.DeprecatedBy);
    //    Version++;
    //}

    private void AddRemarks(string? remarks, FederatedUser createdBy, DateTimeOffset createdDate)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return;
        }
        this.remarks.Add(new RemarksModel(remarks, createdBy, createdDate));
    }

    public record RemarksModel(string Remarks, FederatedUser CreatedBy, DateTimeOffset CreatedDate);

    //private void Append(IEvent @event) { } //(this as IEventSource).PendingEvents.Enqueue(@event);
}
