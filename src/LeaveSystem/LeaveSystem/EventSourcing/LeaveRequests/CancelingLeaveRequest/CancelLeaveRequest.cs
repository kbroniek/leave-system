using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
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
    private readonly CurrentDateService dateService;

    public HandleCancelLeaveRequest(IRepository<LeaveRequest> repository, CurrentDateService dateService)
    {
        this.repository = repository;
        this.dateService = dateService;
    }

    public async Task<Unit> Handle(CancelLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindById(command.LeaveRequestId, cancellationToken)
                             ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(command.LeaveRequestId);

        if (leaveRequest.DateFrom < dateService.GetWithoutTime())
        {
            throw new InvalidOperationException("Canceling of past leave requests is not allowed.");
        }
        leaveRequest.Cancel(command.Remarks, command.CanceledBy);

        await repository.Update(leaveRequest, cancellationToken);

        await repository.SaveChanges(cancellationToken);

        return Unit.Value;
    }
}