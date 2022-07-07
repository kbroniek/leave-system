using GoldenEye.Aggregates;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

namespace LeaveSystem.EventSourcing.LeaveRequests;

public class LeaveRequest : Aggregate
{
    public DateTime DateFrom { get; private set; }

    public DateTime DateTo { get; private set; }

    public int? Hours { get; private set; }

    public Guid? Type { get; private set; }

    public string? Remarks { get; private set; }

    public LeaveRequestStatus Status { get; private set; }

    public FederatedUser CreatedBy { get; private set; }

    //For serialization
    public LeaveRequest() { }

    private LeaveRequest(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks, FederatedUser createdBy)
    {
        var @event = LeaveRequestCreated.Create(leaveRequestId, dateFrom, dateTo, hours, type, remarks, createdBy);
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
        Status = LeaveRequestStatus.Pending;
        CreatedBy = @event.CreatedBy;
        Version++;
    }

    public static LeaveRequest Create(Guid leaveRequestId, DateTime dateFrom, DateTime dateTo, int? hours, Guid? type, string? remarks, FederatedUser createdBy)
        => new(leaveRequestId, dateFrom, dateTo, hours, type, remarks, createdBy);
}

