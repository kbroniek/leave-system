namespace LeaveSystem.Domain.LeaveRequests;

using System.Net;
using System.Runtime.Serialization;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Rejecting;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;

public class LeaveRequest : IEventSource
{
    private readonly List<RemarksModel> remarks = [];

    public Guid Id { get; private set; }

    public DateOnly DateFrom { get; private set; }

    public DateOnly DateTo { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public IReadOnlyCollection<RemarksModel> Remarks => remarks;

    public LeaveRequestStatus Status { get; private set; }

    public LeaveRequestUserDto CreatedBy { get; private set; }

    public LeaveRequestUserDto LastModifiedBy { get; private set; }

    public LeaveRequestUserDto AssignedTo { get; private set; }

    public TimeSpan WorkingHours { get; private set; }

    public int Version { get; private set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset LastModifiedDate { get; set; }

    [IgnoreDataMember]
    Queue<IEvent> IEventSource.PendingEvents { get; } = new Queue<IEvent>();

    public LeaveRequest() { }

    IEventSource IEventSource.Evolve(IEvent @event) => @event switch
    {
        LeaveRequestCreated createdEvent =>
            Apply(createdEvent),
        LeaveRequestAccepted accepted =>
            Apply(accepted),
        LeaveRequestRejected rejected =>
            Apply(rejected),
        LeaveRequestCanceled canceled =>
            Apply(canceled),
        _ => this
    };

    internal virtual Result<LeaveRequest, Error> Pending(
        Guid leaveRequestId,
        DateOnly dateFrom,
        DateOnly dateTo,
        TimeSpan duration,
        Guid leaveTypeId,
        string? remarks,
        LeaveRequestUserDto createdBy,
        LeaveRequestUserDto assignedTo,
        TimeSpan workingHours,
        DateTimeOffset createdDate)
    {
        if (Status is not LeaveRequestStatus.Init)
        {
            return new Error($"Creating leave request in '{Status}' status is not allowed.", HttpStatusCode.UnprocessableEntity);
        }
        var @event = LeaveRequestCreated.Create(
            leaveRequestId,
            dateFrom,
            dateTo,
            duration,
            leaveTypeId,
            remarks,
            createdBy,
            assignedTo,
            workingHours,
            createdDate);

        Append(@event);
        return Apply(@event);
    }

    internal virtual Result<LeaveRequest, Error> Accept(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate)
    {
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Rejected)
        {
            return new Error($"Accepting leave request in '{Status}' status is not allowed.", HttpStatusCode.UnprocessableEntity);
        }
        if (leaveRequestId != Id)
        {
            return new Error($"Accepting leave request in different id is not allowed.", HttpStatusCode.Forbidden);
        }

        var @event = LeaveRequestAccepted.Create(Id, remarks, acceptedBy, createdDate);

        Append(@event);
        return Apply(@event);
    }
    internal Result<LeaveRequest, Error> Reject(Guid leaveRequestId, string? remarks, LeaveRequestUserDto rejectedBy, DateTimeOffset createdDate)
    {
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
        {
            return new Error($"Rejecting leave request in '{Status}' status is not allowed.", HttpStatusCode.UnprocessableEntity);
        }
        if (leaveRequestId != Id)
        {
            return new Error($"Rejecting leave request in different id is not allowed.", HttpStatusCode.Forbidden);
        }

        var @event = LeaveRequestRejected.Create(Id, remarks, rejectedBy, createdDate);

        Append(@event);
        return Apply(@event);
    }

    internal Result<LeaveRequest, Error> Cancel(Guid leaveRequestId, string? remarks, LeaveRequestUserDto canceledBy, DateTimeOffset createdDate)
    {
        if (!string.Equals(CreatedBy.Id, canceledBy.Id, StringComparison.OrdinalIgnoreCase))
        {
            return new Error("Canceling a non-your leave request is not allowed.", HttpStatusCode.BadRequest);
        }
        if (leaveRequestId != Id)
        {
            return new Error($"Canceling leave request in different id is not allowed.", HttpStatusCode.Forbidden);
        }
        if (Status is not LeaveRequestStatus.Pending and not LeaveRequestStatus.Accepted)
        {
            return new Error($"Canceling leave requests in '{Status}' status is not allowed.", HttpStatusCode.UnprocessableEntity);
        }

        var @event = LeaveRequestCanceled.Create(Id, remarks, canceledBy, createdDate);

        Append(@event);
        return Apply(@event);
    }

    //internal void Deprecate(string? remarks, LeaveRequestUser deprecatedBy)
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
        AssignedTo = @event.AssignedTo;
        CreatedDate = @event.CreatedDate;
        LastModifiedDate = @event.CreatedDate;
        Version++;
        return this;
    }

    private LeaveRequest Apply(LeaveRequestAccepted @event)
    {
        Status = LeaveRequestStatus.Accepted;
        AddRemarks(@event.Remarks, @event.AcceptedBy, @event.CreatedDate);
        LastModifiedBy = @event.AcceptedBy;
        LastModifiedDate = @event.CreatedDate;
        Version++;
        return this;
    }

    private LeaveRequest Apply(LeaveRequestRejected @event)
    {
        Status = LeaveRequestStatus.Rejected;
        AddRemarks(@event.Remarks, @event.RejectedBy, @event.CreatedDate);
        LastModifiedBy = @event.RejectedBy;
        Version++;
        return this;
    }

    private LeaveRequest Apply(LeaveRequestCanceled @event)
    {
        Status = LeaveRequestStatus.Canceled;
        AddRemarks(@event.Remarks, @event.CanceledBy, @event.CreatedDate);
        LastModifiedBy = @event.CanceledBy;
        Version++;
        return this;
    }

    //private void Apply(LeaveRequestDeprecated @event)
    //{
    //    LastModifiedBy = @event.DeprecatedBy;
    //    Status = LeaveRequestStatus.Deprecated;
    //    AddRemarks(@event.Remarks, @event.DeprecatedBy);
    //    Version++;
    //}

    private void AddRemarks(string? remarks, LeaveRequestUserDto createdBy, DateTimeOffset createdDate)
    {
        if (string.IsNullOrWhiteSpace(remarks))
        {
            return;
        }
        this.remarks.Add(new RemarksModel(remarks, createdBy, createdDate));
    }

    public record RemarksModel(string Remarks, LeaveRequestUserDto CreatedBy, DateTimeOffset CreatedDate);

    private void Append(IEvent @event) => (this as IEventSource).PendingEvents.Enqueue(@event);
}
