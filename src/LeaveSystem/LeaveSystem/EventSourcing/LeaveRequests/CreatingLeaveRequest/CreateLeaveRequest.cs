using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Db;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }

    public DateTime DateFrom { get; }

    public DateTime DateTo { get; }

    public int? Hours { get; }

    public Guid? Type { get; }

    public string? Remarks { get; }

    public FederatedUser CreatedBy { get; }

    private CreateLeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks, FederatedUser createdBy)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Hours = hours;
        Type = type;
        Remarks = remarks;
        CreatedBy = createdBy;
    }
    public static CreateLeaveRequest Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks, FederatedUser createdBy)
        => new(leaveRequestId, dateFrom, dateTo, hours, type, remarks, createdBy);
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