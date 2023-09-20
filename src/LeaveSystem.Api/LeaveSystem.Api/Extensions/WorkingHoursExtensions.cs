using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Extensions;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Api.Extensions;

public static class WorkingHoursExtensions
{
    public static WorkingHoursDto ToDto(this WorkingHours workingHours) =>
        new(workingHours.UserId, workingHours.DateFrom, workingHours.DateTo,
            workingHours.Duration, workingHours.Id);
}