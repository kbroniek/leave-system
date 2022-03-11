using GoldenEye.Backend.Core.DDD.Aggregates;
using GoldenEye.Backend.Core.DDD.Events;
using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.LeaveRequests.CreatingLeaveRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveSystem.LeaveRequests;

public class LeaveRequest : EventSource
{
    public DateTime DateFrom { get; }

    public DateTime DateTo { get; }

    public int? Hours { get; }

    public string? Type { get; }

    public string? Remarks { get; }
    public int Version { get; }

    //For serialization
    public LeaveRequest() { }

    private LeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
    {
        Id = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Hours = hours;
        Type = type;
        Remarks = remarks;
        Version++;

        var @event = LeaveRequestCreated.Create(leaveRequestId, dateFrom, dateTo, hours, type, remarks);
        Append(@event);
    }

    public static LeaveRequest Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, string? type, string? remarks)
    {
        return new LeaveRequest(leaveRequestId, dateFrom, dateTo, hours, type, remarks);
    }
}

