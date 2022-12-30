using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequestOnBehalf : ICommand
{
    public CreateLeaveRequest CreateLeaveRequest { get; }

    public FederatedUser CreatedByOnBehalf { get; }

    private CreateLeaveRequestOnBehalf(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan? duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy, FederatedUser createdByOnBehalf)
    {
        CreateLeaveRequest = CreateLeaveRequest.Create(
            leaveRequestId,
            dateFrom,
            dateTo,
            duration,
            leaveTypeId,
            remarks,
            createdBy
        );
        CreatedByOnBehalf = createdByOnBehalf;
    }
    public static CreateLeaveRequestOnBehalf Create(Guid? leaveRequestId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, TimeSpan? duration, Guid? leaveTypeId, string? remarks, FederatedUser? createdBy, FederatedUser? createdByOnBehalf)
    {
        return new(
            Guard.Against.NillAndDefault(leaveRequestId),
            Guard.Against.NillAndDefault(dateFrom),
            Guard.Against.NillAndDefault(dateTo),
            duration,
            Guard.Against.NillAndDefault(leaveTypeId),
            remarks,
            Guard.Against.NillAndDefault(createdBy),
            Guard.Against.NillAndDefault(createdByOnBehalf));
    }
}

internal class HandleCreateLeaveRequestOnBehalf :
    ICommandHandler<CreateLeaveRequestOnBehalf>
{
    private readonly IRepository<LeaveRequest> repository;
    private readonly LeaveRequestFactory leaveRequestFactory;

    public HandleCreateLeaveRequestOnBehalf(IRepository<LeaveRequest> repository, LeaveRequestFactory leaveRequestFactory)
    {
        this.repository = repository;
        this.leaveRequestFactory = leaveRequestFactory;
    }

    public async Task<Unit> Handle(CreateLeaveRequestOnBehalf command, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestFactory.Create(command.CreateLeaveRequest, cancellationToken);
        leaveRequest.OnBehalf(command.CreatedByOnBehalf);
        await repository.Add(leaveRequest, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}