namespace LeaveSystem.Domain.LeaveRequests;

using System.Text.Json.Serialization;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;

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

    public static LeaveRequestAccepted Create(Guid leaveRequestId, string? remarks, FederatedUser acceptedBy) =>
        new(leaveRequestId, remarks, acceptedBy);
}
