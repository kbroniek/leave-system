using GoldenEye.Commands;
using GoldenEye.Repositories;
using MediatR;

namespace LeaveSystem.Es.CreatingLeaveRequest;

public class CreateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }

    public DateTime DateFrom { get; }

    public DateTime DateTo { get; }

    public int? Hours { get; }

    public Guid? Type { get; }

    public string? Remarks { get; }

    private CreateLeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Hours = hours;
        Type = type;
        Remarks = remarks;
    }
    public static CreateLeaveRequest Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks)
    {
        return new CreateLeaveRequest(leaveRequestId, dateFrom, dateTo, hours, type, remarks);
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
        var leaveRequest = LeaveRequest.Create(command.LeaveRequestId, command.DateFrom, command.DateTo, command.Hours, command.Type, command.Remarks);
        await repository.Add(leaveRequest, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}