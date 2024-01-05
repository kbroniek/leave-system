using GoldenEye.Aggregates;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;
using LeaveSystem.Periods;
using LeaveSystem.Shared;

namespace LeaveSystem.EventSourcing.WorkingHours;

public class WorkingHours : Aggregate, IDateToNullablePeriod
{
    public string UserId { get; private set; } = null!;
    public DateTimeOffset DateFrom { get; private set; }
    public DateTimeOffset? DateTo { get; private set; }
    public TimeSpan Duration { get; private set; }
    public FederatedUser LastModifiedBy { get; private set; }

    //For serialization
    public WorkingHours() { }

    private WorkingHours(WorkingHoursCreated @event)
    {
        Enqueue(@event);
        Apply(@event);
    }

    public static WorkingHours CreateWorkingHours(WorkingHoursCreated @event) => new(@event);

    internal void Modify(ModifyWorkingHours command)
    {
        var @event = WorkingHoursModified.Create(
            command.WorkingHoursId, command.DateFrom, command.DateTo, command.Duration, command.ModifiedBy);
        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(WorkingHoursModified @event)
    {
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        LastModifiedBy = @event.ModifiedBy;
        Version++;
    }

    private void Apply(WorkingHoursCreated @event)
    {
        Id = @event.WorkingHoursId;
        UserId = @event.UserId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        LastModifiedBy = @event.CreatedBy;
    }
}
