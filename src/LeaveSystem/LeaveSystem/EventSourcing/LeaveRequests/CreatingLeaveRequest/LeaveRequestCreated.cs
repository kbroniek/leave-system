using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
public class LeaveRequestCreated : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public Guid LeaveRequestId { get; }

    public DateTimeOffset DateFrom { get; }

    public DateTimeOffset DateTo { get; }

    public TimeSpan Duration { get; }

    public Guid LeaveTypeId { get; }

    public string? Remarks { get; }

    public FederatedUser CreatedBy { get; }
    
    public TimeSpan WorkingHours { get; }

    [JsonConstructor]
    private LeaveRequestCreated(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy, TimeSpan workingHours)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        LeaveTypeId = leaveTypeId;
        Remarks = remarks;
        CreatedBy = createdBy;
        WorkingHours = workingHours;
    }
    public static LeaveRequestCreated Create(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy, TimeSpan workingHours)
    {
        Guard.Against.InvalidEmail(createdBy.Email, $"{nameof(createdBy)}.{nameof(createdBy.Email)}");
        var dateFromWithoutTime = dateFrom.GetDayWithoutTime();
        var dateToWithoutTime = dateTo.GetDayWithoutTime();
        const int hoursInDayCount = 24;
        const int minHoursInDayCount = 1;
        Guard.Against.OutOfRange(workingHours, nameof(workingHours), TimeSpan.FromHours(minHoursInDayCount), TimeSpan.FromHours(hoursInDayCount));
        return new(leaveRequestId, dateFromWithoutTime, dateToWithoutTime, duration, leaveTypeId, remarks, createdBy, workingHours);
    }
}
