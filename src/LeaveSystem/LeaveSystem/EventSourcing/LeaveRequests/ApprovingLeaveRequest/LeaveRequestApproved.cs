using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.ApprovingLeaveRequest;

public class LeaveRequestApproved : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public string? Remarks { get; }

    public FederatedUser ApprovedBy { get; }

    [JsonConstructor]
    private LeaveRequestApproved(Guid leaveRequestId, string? remarks, FederatedUser approvedBy)
    {
        ApprovedBy = approvedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static LeaveRequestApproved Create(Guid leaveRequestId, string? remarks, FederatedUser approvedBy)
    {
        leaveRequestId = Guard.Against.Default(leaveRequestId);
        approvedBy = Guard.Against.Nill(approvedBy);
        Guard.Against.InvalidEmail(approvedBy.Email, $"{nameof(approvedBy)}.{nameof(approvedBy.Email)}");
        return new(leaveRequestId, remarks, approvedBy);
    }
}
