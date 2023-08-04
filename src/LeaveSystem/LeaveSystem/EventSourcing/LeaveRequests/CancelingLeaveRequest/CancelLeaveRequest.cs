using GoldenEye.Repositories;
using LeaveSystem.Shared;

namespace LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;

public class CancelLeaveRequest : BasicLeaveRequestAction
{
    public FederatedUser CanceledBy => DidBy;

    private CancelLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser canceledBy) : base(leaveRequestId, remarks, canceledBy)
    {
    }

    public static CancelLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? canceledBy)
    {
        var validatedProperties = ValidateProperties(leaveRequestId, canceledBy);
        return new CancelLeaveRequest(validatedProperties.LeaveRequestId, remarks, validatedProperties.DidBy);
    }
}

internal class HandleCancelLeaveRequest : HandleBasicLeaveRequestAction<CancelLeaveRequest>
{
    public HandleCancelLeaveRequest(IRepository<LeaveRequest> repository): base(repository)
    {
    }
}