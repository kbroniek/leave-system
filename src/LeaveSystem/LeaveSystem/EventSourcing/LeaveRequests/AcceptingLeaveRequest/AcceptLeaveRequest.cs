using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;

public class AcceptLeaveRequest : BasicLeaveRequestAction
{
    public FederatedUser AcceptedBy => DidBy;

    private AcceptLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser acceptedBy)
        : base(leaveRequestId, remarks, acceptedBy)
    {
    }

    public static AcceptLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? acceptedBy)
    {
        var validatedProperties = ValidateProperties(leaveRequestId, acceptedBy);
        return new AcceptLeaveRequest(validatedProperties.LeaveRequestId, remarks, validatedProperties.DidBy);
    }
}

internal class HandleAcceptLeaveRequest : HandleBasicLeaveRequestAction<AcceptLeaveRequest>
{
    public HandleAcceptLeaveRequest(IRepository<LeaveRequest> repository) : base(repository)
    {
    }

    public override async Task<Unit> Handle(AcceptLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await GetLeaveRequestAsync(command, cancellationToken);
        leaveRequest.Accept(command.Remarks, command.AcceptedBy);
        await UpdateAndSaveChangesAsync(leaveRequest, cancellationToken);
        return Unit.Value;
    }
}