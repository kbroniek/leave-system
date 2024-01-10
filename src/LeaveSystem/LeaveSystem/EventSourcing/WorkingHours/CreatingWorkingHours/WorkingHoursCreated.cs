using GoldenEye.Backend.Core.DDD.Events;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;

public class WorkingHoursCreated : IEvent
{
    public Guid StreamId => WorkingHoursId;
    public Guid WorkingHoursId { get; }
    public string UserId { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset? DateTo { get; }
    public TimeSpan Duration { get; }
    public FederatedUser CreatedBy { get; }

    [JsonConstructor]
    private WorkingHoursCreated(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo, TimeSpan duration, FederatedUser createdBy)
    {
        WorkingHoursId = workingHoursId;
        UserId = userId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        CreatedBy = createdBy;
    }

    public static WorkingHoursCreated Create(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo, TimeSpan duration, FederatedUser createdBy)
    {
        if (dateFrom > dateTo)
        {
            throw new ArgumentOutOfRangeException(nameof(dateFrom), "Date from has to be less than date to.");
        }
        return new(
            workingHoursId,
            userId,
            dateFrom,
            dateTo,
            duration,
            createdBy
        );
    }
}
