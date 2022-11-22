using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
public class LeaveRequestBehalfOnCreated : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public FederatedUser CreatedByBehalfOn { get; }

    [JsonConstructor]
    private LeaveRequestBehalfOnCreated(Guid leaveRequestId, FederatedUser createdByBehalfOn)
    {
        LeaveRequestId = leaveRequestId;
        CreatedByBehalfOn = createdByBehalfOn;
    }

    public static LeaveRequestBehalfOnCreated Create(Guid leaveRequestId,  FederatedUser createdByBehalfOn)
    {
        Guard.Against.InvalidEmail(createdByBehalfOn.Email, $"{nameof(createdByBehalfOn)}.{nameof(createdByBehalfOn.Email)}");
        return new(leaveRequestId, createdByBehalfOn);
    }
}
