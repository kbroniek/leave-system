using GoldenEye.Backend.Core.DDD.Commands;
using MediatR;

namespace LeaveSystem.LeaveRequests.AddingLeaveRequest;

public class AddLeaveRequest : ICommand
{
    public DateTime DateFrom { get; }

    public DateTime DateTo { get; }

    public int? Hours { get; }

    public string? Type { get; }

    public string? Remarks { get; }

    private AddLeaveRequest(DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
        Hours = hours;
        Type = type;
        Remarks = remarks;
    }
    public static AddLeaveRequest Create(DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
    {
        return new AddLeaveRequest(dateFrom, dateTo, hours, type, remarks);
    }
}


internal class HandleAddLeaveRequest :
    ICommandHandler<AddLeaveRequest>
{

    public HandleAddLeaveRequest()
    {
    }

    public Task<Unit> Handle(AddLeaveRequest command, CancellationToken cancellationToken)
    {
        return Task.FromResult(Unit.Value);
    }
}