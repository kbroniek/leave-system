using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Date;
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
        var canceledByNotNull = Guard.Against.Nill(canceledBy);
        Guard.Against.InvalidEmail(canceledByNotNull.Email, $"{nameof(canceledBy)}.{nameof(canceledByNotNull.Email)}");
        return new(Guard.Against.NillAndDefault(leaveRequestId), remarks, canceledByNotNull);
    }
}

internal class HandleCancelLeaveRequest :
    ICommandHandler<CancelLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;
    private readonly DateService dateService;

    public HandleCancelLeaveRequest(IRepository<LeaveRequest> repository, DateService dateService)
    {
        this.repository = repository;
        this.dateService = dateService;
    }

    public async Task<Unit> Handle(CancelLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindByIdAsync(command.LeaveRequestId, cancellationToken)
                             ?? throw GoldenEye.Backend.Core.Exceptions.NotFoundException.For<LeaveRequest>(command.LeaveRequestId);

        leaveRequest.Cancel(command.Remarks, command.CanceledBy, dateService.UtcNowWithoutTime());

        await repository.UpdateAsync(leaveRequest, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
