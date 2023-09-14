using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Extensions;

public static class WorkingHoursExtensions
{
    public static WorkingHoursDto ToDto(this WorkingHours workingHours) =>
        new(workingHours.UserId, workingHours.DateFrom, workingHours.DateTo,
            workingHours.Duration, workingHours.Status);
}