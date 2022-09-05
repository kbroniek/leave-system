using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;

public class AcceptLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }
    public string? Remarks { get; }
    public FederatedUser AcceptedBy { get; }

    private AcceptLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser acceptedBy)
    {
        AcceptedBy = acceptedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static AcceptLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? acceptedBy)
    {
        leaveRequestId = Guard.Against.Nill(leaveRequestId);
        acceptedBy = Guard.Against.Nill(acceptedBy);
        return new(leaveRequestId.Value, remarks, acceptedBy);
    }
}

internal class HandleAcceptLeaveRequest :
    ICommandHandler<AcceptLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleAcceptLeaveRequest(IRepository<LeaveRequest> repository)
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(AcceptLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindById(command.LeaveRequestId, cancellationToken)
                             ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(command.LeaveRequestId);

        leaveRequest.Accept(command.Remarks, command.AcceptedBy);

        await repository.Update(leaveRequest, cancellationToken);

        await repository.SaveChanges(cancellationToken);

        return Unit.Value;
    }
}
