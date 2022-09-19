using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using Marten.Events.Aggregation;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class LeaveRequestShortInfo
{
    public Guid Id { get; set; }

    public DateTimeOffset DateFrom { get; private set; }

    public DateTimeOffset DateTo { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public LeaveRequestStatus Status { get; private set; }

    public FederatedUser CreatedBy { get; private set; }

    public void Apply(LeaveRequestCreated @event)
    {
        Id = @event.LeaveRequestId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        LeaveTypeId = @event.LeaveTypeId;
        Status = LeaveRequestStatus.Pending;
        CreatedBy = @event.CreatedBy;
    }

    public void Apply(LeaveRequestAccepted _)
    {
        Status = LeaveRequestStatus.Accepted;
    }

    public void Apply(LeaveRequestRejected _)
    {
        Status = LeaveRequestStatus.Rejected;
    }

    public void Apply(LeaveRequestCancelled _)
    {
        Status = LeaveRequestStatus.Cancelled;
    }
}

public class LeaveRequestShortInfoProjection : SingleStreamAggregation<LeaveRequestShortInfo>
{
    public LeaveRequestShortInfoProjection()
    {
        ProjectEvent<LeaveRequestCreated>((item, @event) => item.Apply(@event));

        ProjectEvent<LeaveRequestAccepted>((item, @event) => item.Apply(@event));

        ProjectEvent<LeaveRequestRejected>((item, @event) => item.Apply(@event));

        ProjectEvent<LeaveRequestCancelled>((item, @event) => item.Apply(@event));
    }
}

