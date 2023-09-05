using System.ComponentModel.DataAnnotations;
using Marten;

namespace LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;

public class CreateWorkingHoursValidator
{
    private readonly IDocumentSession documentSession;

    public CreateWorkingHoursValidator(IDocumentSession documentSession)
    {
        this.documentSession = documentSession;
    }

    public virtual void ValidateWorkingHoursUnique(WorkingHoursCreated @event)
    {
        var userId = @event.UserId;
        var workingHoursForThisUserExists = documentSession.Events
            .QueryRawEventDataOnly<WorkingHoursCreated>().Any(wh => wh.UserId == userId);
        if (workingHoursForThisUserExists)
        {
            throw new ValidationException("Cannot create a new working hours, because this user have already working hours");
        }
    }
}