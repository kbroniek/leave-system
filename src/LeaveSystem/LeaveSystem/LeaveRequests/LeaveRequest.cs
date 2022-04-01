using GoldenEye.Aggregates;
using LeaveSystem.LeaveRequests.CreatingLeaveRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.LeaveRequests;

public class LeaveRequest : Aggregate
{
    public DateTime DateFrom { get; private set; }

    public DateTime DateTo { get; private set; }

    public int? Hours { get; private set; }

    public string? Type { get; private set; }

    public string? Remarks { get; private set; }

    //For serialization
    public LeaveRequest() { }

    private LeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
    {
        var @event = LeaveRequestCreated.Create(leaveRequestId, dateFrom, dateTo, hours, type, remarks);
        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(LeaveRequestCreated @event)
    {
        Id = @event.LeaveRequestId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Hours = @event.Hours;
        Type = @event.Type;
        Remarks = @event.Remarks;
        Version++;
    }

    public static LeaveRequest Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
    {
        return new LeaveRequest(leaveRequestId, dateFrom, dateTo, hours, type, remarks);
    }
}

