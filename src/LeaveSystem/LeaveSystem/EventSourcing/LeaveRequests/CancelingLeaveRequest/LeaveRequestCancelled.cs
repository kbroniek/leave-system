using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;

public class LeaveRequestCancelled : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public string? Remarks { get; }

    public FederatedUser CancelledBy { get; }

    [JsonConstructor]
    private LeaveRequestCancelled(Guid leaveRequestId, string? remarks, FederatedUser cancelledBy)
    {
        CancelledBy = cancelledBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static LeaveRequestCancelled Create(Guid leaveRequestId, string? remarks, FederatedUser cancelledBy)
    {
        leaveRequestId = Guard.Against.Default(leaveRequestId);
        cancelledBy = Guard.Against.Nill(cancelledBy);
        Guard.Against.InvalidEmail(cancelledBy.Email, $"{nameof(cancelledBy)}.{nameof(cancelledBy.Email)}");
        return new(leaveRequestId, remarks, cancelledBy);
    }
}