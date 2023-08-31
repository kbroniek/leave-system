using GoldenEye.Aggregates;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.EventSourcing.WorkingHours;

public class WorkingHours : Aggregate
{
    public string UserId { get; private set; }
    public DateTimeOffset DateFrom { get; private set; }
    public DateTimeOffset DateTo { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public WorkingHoursStatus Status { get; private set; }

    private WorkingHours(WorkingHoursCreated @event)
    {
        Enqueue(@event);
        Apply(@event);
    }

    public WorkingHours()
    {
    }

    public static WorkingHours CreateWorkingHours(WorkingHoursCreated @event) => new(@event);

    private void Apply(WorkingHoursCreated @event)
    {
        Id = @event.StreamId;
        UserId = @event.UserId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        Status = WorkingHoursStatus.Current;
    }
}