using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }

    public DateTimeOffset DateFrom { get; }

    public DateTimeOffset DateTo { get; }

    public TimeSpan? Duration { get; }

    public Guid LeaveTypeId { get; }

    public string? Remarks { get; }

    public FederatedUser CreatedBy { get; }

    private CreateLeaveRequest(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan? duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        LeaveTypeId = leaveTypeId;
        Remarks = remarks;
        CreatedBy = createdBy;
    }
    public static CreateLeaveRequest Create(Guid? leaveRequestId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, TimeSpan? duration, Guid? leaveTypeId, string? remarks, FederatedUser? createdBy) =>
        new(
            Guard.Against.NillAndDefault(leaveRequestId),
            Guard.Against.NillAndDefault(dateFrom),
            Guard.Against.NillAndDefault(dateTo),
            duration,
            Guard.Against.NillAndDefault(leaveTypeId),
            remarks,
            Guard.Against.NillAndDefault(createdBy));
}

internal class HandleCreateLeaveRequest :
    ICommandHandler<CreateLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;
    private readonly LeaveRequestFactory leaveRequestFactory;

    public HandleCreateLeaveRequest(IRepository<LeaveRequest> repository, LeaveRequestFactory leaveRequestFactory)
    {
        this.repository = repository;
        this.leaveRequestFactory = leaveRequestFactory;
    }

    public async Task<Unit> Handle(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestFactory.Create(command, cancellationToken);
        await repository.AddAsync(leaveRequest, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
