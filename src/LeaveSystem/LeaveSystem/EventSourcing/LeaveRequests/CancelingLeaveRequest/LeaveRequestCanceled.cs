using GoldenEye.Backend.Core.DDD.Events;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;

public class LeaveRequestCanceled : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public string? Remarks { get; }

    public FederatedUser CanceledBy { get; }

    [JsonConstructor]
    private LeaveRequestCanceled(Guid leaveRequestId, string? remarks, FederatedUser canceledBy)
    {
        CanceledBy = canceledBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static LeaveRequestCanceled Create(Guid leaveRequestId, string? remarks, FederatedUser canceledBy)
    {
        return new(leaveRequestId, remarks, canceledBy);
    }
}
