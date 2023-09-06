using GoldenEye.Events;
using Newtonsoft.Json;

namespace LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;

public class WorkingHoursDeprecated : IEvent
{

    public Guid StreamId => WorkingHoursId;
    
    public Guid WorkingHoursId { get; }
    
    [JsonConstructor]
    private WorkingHoursDeprecated(Guid workingHoursId)
    {
        WorkingHoursId = workingHoursId;
    }

    public static WorkingHoursDeprecated Create(Guid workingHoursId) =>
        new(workingHoursId);
}
