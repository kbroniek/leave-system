using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests;

public class BasicLeaveRequestAction : ICommand
{
    public Guid LeaveRequestId { get;}
    public string? Remarks { get;}
    public FederatedUser DidBy { get;}

    internal BasicLeaveRequestAction(Guid leaveRequestId, string? remarks, FederatedUser didBy)
    {
        DidBy = didBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    protected static ValidatedProperties ValidateProperties(Guid? leaveRequestId, FederatedUser? didBy)
    {
        var acceptedByNotNull = Guard.Against.Nill(didBy);
        Guard.Against.InvalidEmail(acceptedByNotNull.Email, $"{nameof(didBy)}.{nameof(acceptedByNotNull.Email)}");
        var leaveRequestIdNotNull = Guard.Against.NillAndDefault(leaveRequestId);
        return new ValidatedProperties(leaveRequestIdNotNull, acceptedByNotNull);
    }

    protected record ValidatedProperties(Guid LeaveRequestId, FederatedUser DidBy);
}

internal abstract class HandleBasicLeaveRequestAction<T> :
    ICommandHandler<T> where T : BasicLeaveRequestAction
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleBasicLeaveRequestAction(IRepository<LeaveRequest> repository)
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(T command, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindById(command.LeaveRequestId, cancellationToken)
                           ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(command.LeaveRequestId);

        leaveRequest.Accept(command.Remarks, command.DidBy);

        await repository.Update(leaveRequest, cancellationToken);

        await repository.SaveChanges(cancellationToken);

        return Unit.Value;
    }
}