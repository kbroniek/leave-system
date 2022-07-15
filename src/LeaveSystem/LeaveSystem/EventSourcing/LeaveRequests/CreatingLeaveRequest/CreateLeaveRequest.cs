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
    public static CreateLeaveRequest Create(Guid? leaveRequestId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, TimeSpan? duration, Guid? leaveTypeId, string? remarks, FederatedUser? createdBy)
    {
        leaveRequestId = Guard.Against.Nill(leaveRequestId);
        dateFrom = Guard.Against.Nill(dateFrom);
        dateTo = Guard.Against.Nill(dateTo);
        leaveTypeId = Guard.Against.Nill(leaveTypeId);
        createdBy = Guard.Against.Nill(createdBy);
        return new(leaveRequestId.Value, dateFrom.Value, dateTo.Value, duration, leaveTypeId.Value, remarks, createdBy);
    }
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
        var leaveRequest = await leaveRequestFactory.Create(command);
        await repository.Add(leaveRequest, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}