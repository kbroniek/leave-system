using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;

public class RejectLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }
    public string? Remarks { get; }
    public FederatedUser RejectedBy { get; }

    private RejectLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser rejectedBy)
    {
        RejectedBy = rejectedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static RejectLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? rejectedBy)
    {
        var rejectedByNotNull = Guard.Against.Nill(rejectedBy);
        Guard.Against.InvalidEmail(rejectedByNotNull.Email, $"{nameof(rejectedBy)}.{nameof(rejectedByNotNull.Email)}");
        return new(Guard.Against.NillAndDefault(leaveRequestId), remarks, rejectedByNotNull);
    }
}

internal class HandleRejectLeaveRequest :
    ICommandHandler<RejectLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleRejectLeaveRequest(IRepository<LeaveRequest> repository) => this.repository = repository;

    public async Task<Unit> Handle(RejectLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindByIdAsync(command.LeaveRequestId, cancellationToken)
                             ?? throw GoldenEye.Backend.Core.Exceptions.NotFoundException.For<LeaveRequest>(command.LeaveRequestId);

        leaveRequest.Reject(command.Remarks, command.RejectedBy);

        await repository.UpdateAsync(leaveRequest, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
