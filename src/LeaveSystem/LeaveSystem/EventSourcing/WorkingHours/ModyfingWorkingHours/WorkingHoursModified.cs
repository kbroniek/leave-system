using GoldenEye.Backend.Core.DDD.Events;
using LeaveSystem.Shared;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;

public class WorkingHoursModified : IEvent
{
    public Guid StreamId => WorkingHoursId;
    public Guid WorkingHoursId { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset? DateTo { get; }
    public TimeSpan Duration { get; }
    public FederatedUser ModifiedBy { get; }

    [JsonConstructor]
    private WorkingHoursModified(Guid workingHoursId, DateTimeOffset dateFrom, DateTimeOffset? dateTo,
        TimeSpan duration, FederatedUser modifiedBy)
    {
        WorkingHoursId = workingHoursId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        ModifiedBy = modifiedBy;
    }

    public static WorkingHoursModified Create(Guid workingHoursId, DateTimeOffset dateFrom,
        DateTimeOffset? dateTo, TimeSpan duration, FederatedUser modifiedBy)
    {
        if (dateFrom > dateTo)
        {
            throw new ArgumentOutOfRangeException(nameof(dateFrom), "Date from has to be less than date to.");
        }

        return new(
            workingHoursId,
            dateFrom,
            dateTo,
            duration,
            modifiedBy
        );
    }
}
