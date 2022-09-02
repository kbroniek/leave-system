using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Db;
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
        leaveRequestId = Guard.Against.Default(leaveRequestId);
        rejectedBy = Guard.Against.Nill(rejectedBy);
        Guard.Against.InvalidEmail(rejectedBy.Email, $"{nameof(rejectedBy)}.{nameof(rejectedBy.Email)}");
        return new(leaveRequestId, remarks, rejectedBy);
    }
}

