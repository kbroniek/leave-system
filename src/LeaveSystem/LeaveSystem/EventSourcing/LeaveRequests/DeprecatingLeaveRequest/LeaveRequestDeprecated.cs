using GoldenEye.Events;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.DeprecatingLeaveRequest;

public class LeaveRequestDeprecated : IEvent
{
    public Guid StreamId => LeaveRequestId;
    public Guid LeaveRequestId { get; }
    public string? Remarks { get; }
    public FederatedUser DeprecatedBy { get; }

    [JsonConstructor]
    private LeaveRequestDeprecated(Guid leaveRequestId, string? remarks, FederatedUser deprecatedBy)
    {
        DeprecatedBy = deprecatedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static LeaveRequestDeprecated Create(Guid leaveRequestId, string? remarks, FederatedUser canceledBy) => 
        new(leaveRequestId, remarks, canceledBy);
}