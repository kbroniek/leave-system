using GoldenEye.Aggregates;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.EventSourcing.WorkingHours;

public class WorkingHours : Aggregate
{
    public string UserId { get; private set; }
    public DateTimeOffset DateFrom { get; private set; }
    public DateTimeOffset? DateTo { get; private set; }
    public TimeSpan Duration { get; private set; }
    public WorkingHoursStatus Status { get; private set; }
    public FederatedUser ModifiedBy { get; private set; }

    //For serialization
    public WorkingHours() { }

    private WorkingHours(WorkingHoursCreated @event)
    {
        Enqueue(@event);
        Apply(@event);
    }

    public static WorkingHours CreateWorkingHours(WorkingHoursCreated @event) => new(@event);

    internal void Deprecate()
    {
        if (Status == WorkingHoursStatus.Deprecated)
        {
            throw new InvalidOperationException("Deprecating deprecated working hours is not allowed");
        }
        var @event = WorkingHoursDeprecated.Create(Id);
        Apply(@event);
        Enqueue(@event);
    }

    private void Apply(WorkingHoursDeprecated _)
    {
        Status = WorkingHoursStatus.Deprecated;
        Version++;
    }

    private void Apply(WorkingHoursCreated @event)
    {
        Id = @event.WorkingHoursId;
        UserId = @event.UserId;
        DateFrom = @event.DateFrom;
        DateTo = @event.DateTo;
        Duration = @event.Duration;
        Status = WorkingHoursStatus.Current;
    }
}