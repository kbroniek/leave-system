using GoldenEye.Commands;
using GoldenEye.Repositories;
using MediatR;

namespace LeaveSystem.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }

    public DateTime DateFrom { get; }

    public DateTime DateTo { get; }

    public int? Hours { get; }

    public string? Type { get; }

    public string? Remarks { get; }

    private CreateLeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Hours = hours;
        Type = type;
        Remarks = remarks;
    }
    public static CreateLeaveRequest Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
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

    public Task<Unit> Handle(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        //var leaveRequest = LeaveRequest.Create(command.LeaveRequestId, command.DateFrom, command.DateTo, command.Hours, command.Type, command.Remarks);
        //await repository.AddAsync(leaveRequest, cancellationToken);
        //await repository.SaveChangesAsync(cancellationToken);
        return Task.FromResult(Unit.Value);
    }
}