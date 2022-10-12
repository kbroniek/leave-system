using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;

public class CancelLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }
    public string? Remarks { get; }
    public FederatedUser CanceledBy { get; }

    private CancelLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser canceledBy)
    {
        CanceledBy = canceledBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static CancelLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? canceledBy)
    {
        leaveRequestId = Guard.Against.Nill(leaveRequestId);
        canceledBy = Guard.Against.Nill(canceledBy);
        return new(leaveRequestId.Value, remarks, canceledBy.Value);
    }
}

internal class HandleCancelLeaveRequest :
    ICommandHandler<CancelLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleCancelLeaveRequest(IRepository<LeaveRequest> repository)
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(CancelLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindById(command.LeaveRequestId, cancellationToken)
                             ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(command.LeaveRequestId);

        leaveRequest.Cancel(command.Remarks, command.CanceledBy);

        await repository.Update(leaveRequest, cancellationToken);

        await repository.SaveChanges(cancellationToken);

        return Unit.Value;
    }
}