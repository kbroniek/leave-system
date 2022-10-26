using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;

public class LeaveRequestRejected : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public string? Remarks { get; }

    public FederatedUser RejectedBy { get; }

    [JsonConstructor]
    private LeaveRequestRejected(Guid leaveRequestId, string? remarks, FederatedUser rejectedBy)
    {
        RejectedBy = rejectedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static LeaveRequestRejected Create(Guid leaveRequestId, string? remarks, FederatedUser rejectedBy)
    {
        return new(leaveRequestId, remarks, rejectedBy);
    }
}

