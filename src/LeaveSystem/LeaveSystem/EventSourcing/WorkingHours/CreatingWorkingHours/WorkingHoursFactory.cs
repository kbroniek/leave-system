namespace LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;

public class WorkingHoursFactory
{
    public virtual WorkingHours Create(CreateWorkingHours command)
    {
        var @event = WorkingHoursCreated.Create(
            command.WorkingHoursId, command.UserId, command.DateFrom, command.DateTo, command.Duration, command.CreatedBy);
        return WorkingHours.CreateWorkingHours(@event);
    }
}