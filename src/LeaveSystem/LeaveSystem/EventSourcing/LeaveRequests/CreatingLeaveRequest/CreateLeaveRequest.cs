using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }

    public DateTime DateFrom { get; }

    public DateTime DateTo { get; }

    public int? Hours { get; }

    public Guid Type { get; }

    public string? Remarks { get; }

    public FederatedUser CreatedBy { get; }

    private CreateLeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid type, string? remarks, FederatedUser createdBy)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Hours = hours;
        Type = type;
        Remarks = remarks;
        CreatedBy = createdBy;
    }
    public static CreateLeaveRequest Create(Guid? leaveRequestId, DateTime? dateFrom, DateTime? dateTo, int? hours, Guid? type, string? remarks, FederatedUser? createdBy)
    {
        leaveRequestId = Guard.Against.Nill(leaveRequestId);
        dateFrom = Guard.Against.Nill(dateFrom);
        dateTo = Guard.Against.Nill(dateTo);
        type = Guard.Against.Nill(type);
        createdBy = Guard.Against.Nill(createdBy);
        return new(leaveRequestId.Value, dateFrom.Value, dateTo.Value, hours, type.Value, remarks, createdBy);
    }
}

internal class HandleCreateLeaveRequest :
    ICommandHandler<CreateLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleCreateLeaveRequest(IRepository<LeaveRequest> repository)
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = LeaveRequest.Create(command.LeaveRequestId, command.DateFrom, command.DateTo, command.Hours, command.Type, command.Remarks, command.CreatedBy);
        await repository.Add(leaveRequest, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}