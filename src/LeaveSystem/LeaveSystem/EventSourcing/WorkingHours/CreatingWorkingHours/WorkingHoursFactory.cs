namespace LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;

public class WorkingHoursFactory
{
    private readonly CreateWorkingHoursValidator validator;

    public WorkingHoursFactory(CreateWorkingHoursValidator validator)
    {
        this.validator = validator;
    }

    public virtual WorkingHours Create(CreateWorkingHours command)
    {
        var @event = WorkingHoursCreated.Create(command.UserId, command.DateFrom, command.DateTo, command.Duration);
        validator.ValidateWorkingHoursUnique(@event);
        return WorkingHours.CreateWorkingHours(@event);
    }
}