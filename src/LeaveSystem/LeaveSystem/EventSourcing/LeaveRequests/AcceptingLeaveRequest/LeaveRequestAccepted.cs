using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;

public class LeaveRequestAccepted : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public string? Remarks { get; }

    public FederatedUser AcceptedBy { get; }

    [JsonConstructor]
    private LeaveRequestAccepted(Guid leaveRequestId, string? remarks, FederatedUser acceptedBy)
    {
        AcceptedBy = acceptedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static LeaveRequestAccepted Create(Guid leaveRequestId, string? remarks, FederatedUser acceptedBy)
    {
        leaveRequestId = Guard.Against.Default(leaveRequestId);
        acceptedBy = Guard.Against.Nill(acceptedBy);
        Guard.Against.InvalidEmail(acceptedBy.Email, $"{nameof(acceptedBy)}.{nameof(acceptedBy.Email)}");
        return new(leaveRequestId, remarks, acceptedBy);
    }
}
