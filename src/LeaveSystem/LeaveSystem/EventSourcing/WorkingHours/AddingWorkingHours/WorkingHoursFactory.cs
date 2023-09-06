namespace LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;

public class WorkingHoursFactory
{
    public virtual WorkingHours Create(AddWorkingHours command)
    {
        var @event = WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom, command.DateTo, command.Duration);
        return WorkingHours.CreateWorkingHours(@event);
    }
}