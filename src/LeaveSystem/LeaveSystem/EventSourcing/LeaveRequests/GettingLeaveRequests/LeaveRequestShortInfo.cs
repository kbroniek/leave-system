using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.DeprecatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using Marten.Events.Aggregation;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class LeaveRequestShortInfo
{
    public Guid Id { get; private set; }

    public DateTimeOffset DateFrom { get; private set; }

    public DateTimeOffset DateTo { get; private set; }

    public TimeSpan Duration { get; private set; }

    public Guid LeaveTypeId { get; private set; }

    public LeaveRequestStatus Status { get; private set; }

    public FederatedUser CreatedBy { get; private set; }

    internal void Apply(LeaveRequestCreated @event)
    {
        Id = @event.LeaveRequestId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        LeaveTypeId = @event.LeaveTypeId;
        Status = LeaveRequestStatus.Pending;
        CreatedBy = @event.CreatedBy;
    }

    internal void Apply(LeaveRequestAccepted _) => Status = LeaveRequestStatus.Accepted;

    internal void Apply(LeaveRequestRejected _) => Status = LeaveRequestStatus.Rejected;

    internal void Apply(LeaveRequestCanceled _) => Status = LeaveRequestStatus.Canceled;

    internal void Apply(LeaveRequestDeprecated _) => Status = LeaveRequestStatus.Deprecated;

    public class LeaveRequestShortInfoProjection : SingleStreamProjection<LeaveRequestShortInfo>
    {
        public LeaveRequestShortInfoProjection()
        {
            ProjectEvent<LeaveRequestCreated>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestAccepted>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestRejected>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestCanceled>((item, @event) => item.Apply(@event));

            ProjectEvent<LeaveRequestDeprecated>((item, @event) => item.Apply(@event));
        }
    }
}
