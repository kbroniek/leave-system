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

    [JsonConstructor]
    private LeaveRequestCreated(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        LeaveTypeId = leaveTypeId;
        Remarks = remarks;
        CreatedBy = createdBy;
    }
    public static LeaveRequestCreated Create(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy)
    {
        Guard.Against.InvalidEmail(createdBy.Email, $"{nameof(createdBy)}.{nameof(createdBy.Email)}");
        var dateFromWithoutTime = dateFrom.GetDayWithoutTime();
        var dateToWithoutTime = dateTo.GetDayWithoutTime();
        var now = DateTimeOffset.UtcNow;
        var firstDay = now.GetFirstDayOfYear();
        var lastDay = now.GetLastDayOfYear();
        Guard.Against.OutOfRange(dateFromWithoutTime, nameof(dateFrom), firstDay, lastDay);
        Guard.Against.OutOfRange(dateToWithoutTime, nameof(dateTo), firstDay, lastDay);

        if (dateFromWithoutTime > dateToWithoutTime)
        {
            throw new ArgumentOutOfRangeException(nameof(dateFrom), "Date from has to be less than date to.");
        }

        return new(leaveRequestId, dateFromWithoutTime, dateToWithoutTime, duration, leaveTypeId, remarks, createdBy);
    }
}
