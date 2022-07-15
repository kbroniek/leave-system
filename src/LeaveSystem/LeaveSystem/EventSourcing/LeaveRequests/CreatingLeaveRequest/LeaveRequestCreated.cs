using Ardalis.GuardClauses;
using GoldenEye.Events;
using LeaveSystem.Db;
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
    private LeaveRequestCreated(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan duration, Guid type, string? remarks, FederatedUser createdBy)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        LeaveTypeId = type;
        Remarks = remarks;
        CreatedBy = createdBy;
    }
    public static LeaveRequestCreated Create(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan duration, Guid type, string? remarks, FederatedUser createdBy)
    {
        leaveRequestId = Guard.Against.Default(leaveRequestId);
        dateFrom = Guard.Against.Default(dateFrom);
        dateTo = Guard.Against.Default(dateTo);
        type = Guard.Against.Default(type);
        duration = Guard.Against.Default(duration);

        var now = DateTimeOffset.UtcNow;
        var firstDay = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lastDay = new DateTimeOffset(now.Year, 12, 31, 23, 59, 59, 999, TimeSpan.Zero);
        Guard.Against.OutOfRange(dateFrom, nameof(dateFrom), firstDay, lastDay);
        Guard.Against.OutOfRange(dateTo, nameof(dateTo), firstDay, lastDay);

        if (dateFrom > dateTo)
        {
            throw new ArgumentOutOfRangeException(nameof(dateFrom), "Date from has to be less than date to.");
        }

        return new(leaveRequestId, dateFrom.UtcDateTime, dateTo.UtcDateTime, duration, type, remarks, createdBy);
    }
}
