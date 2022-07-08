using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }

    public DateTime DateFrom { get; }

    public DateTime DateTo { get; }

    public TimeSpan? Duration { get; }

    public Guid Type { get; }

    public string? Remarks { get; }

    public FederatedUser CreatedBy { get; }

    private CreateLeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, TimeSpan? duration, Guid type, string? remarks, FederatedUser createdBy)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        Type = type;
        Remarks = remarks;
        CreatedBy = createdBy;
    }
    public static CreateLeaveRequest Create(Guid? leaveRequestId, DateTime? dateFrom, DateTime? dateTo, TimeSpan? duration, Guid? type, string? remarks, FederatedUser? createdBy)
    {
        leaveRequestId = Guard.Against.Nill(leaveRequestId);
        dateFrom = Guard.Against.Nill(dateFrom);
        dateTo = Guard.Against.Nill(dateTo);
        type = Guard.Against.Nill(type);
        createdBy = Guard.Against.Nill(createdBy);
        return new(leaveRequestId.Value, dateFrom.Value, dateTo.Value, duration, type.Value, remarks, createdBy);
    }
}

internal class HandleCreateLeaveRequest :
    ICommandHandler<CreateLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;
    private readonly LeaveRequestFactory leaveRequestCreator;

    public HandleCreateLeaveRequest(IRepository<LeaveRequest> repository, LeaveRequestFactory leaveRequestFactory)
    {
        this.repository = repository;
        this.leaveRequestCreator = leaveRequestFactory;
    }

    public async Task<Unit> Handle(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestCreator.Create(command);
        await repository.Add(leaveRequest, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}