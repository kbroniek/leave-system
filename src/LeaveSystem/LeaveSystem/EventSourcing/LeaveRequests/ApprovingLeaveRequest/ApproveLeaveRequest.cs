using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.ApprovingLeaveRequest;

public class ApproveLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }
    public string? Remarks { get; }
    public FederatedUser ApprovedBy { get; }

    private ApproveLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser approvedBy)
    {
        ApprovedBy = approvedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static ApproveLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? approvedBy)
    {
        leaveRequestId = Guard.Against.Nill(leaveRequestId);
        approvedBy = Guard.Against.Nill(approvedBy);
        return new(leaveRequestId.Value, remarks, approvedBy);
    }
}

internal class HandleApproveLeaveRequest :
    ICommandHandler<ApproveLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleApproveLeaveRequest(IRepository<LeaveRequest> repository)
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(ApproveLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindById(command.LeaveRequestId, cancellationToken)
                             ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(command.LeaveRequestId);

        leaveRequest.Approve(command.Remarks, command.ApprovedBy);

        await repository.Update(leaveRequest, cancellationToken);

        await repository.SaveChanges(cancellationToken);

        return Unit.Value;
    }
}
