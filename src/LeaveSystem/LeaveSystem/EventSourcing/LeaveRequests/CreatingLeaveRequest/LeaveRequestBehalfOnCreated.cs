using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
public class LeaveRequestOnBehalfCreated : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public FederatedUser CreatedByOnBehalf { get; }

    [JsonConstructor]
    private LeaveRequestOnBehalfCreated(Guid leaveRequestId, FederatedUser createdByOnBehalf)
    {
        LeaveRequestId = leaveRequestId;
        CreatedByOnBehalf = createdByOnBehalf;
    }

    public static LeaveRequestOnBehalfCreated Create(Guid leaveRequestId,  FederatedUser createdByOnBehalf)
    {
        Guard.Against.InvalidEmail(createdByOnBehalf.Email, $"{nameof(createdByOnBehalf)}.{nameof(createdByOnBehalf.Email)}");
        return new(leaveRequestId, createdByOnBehalf);
    }
}
