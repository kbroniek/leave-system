using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class WorkingHoursExtensions
{
    public static AddWorkingHoursDto ToAddWorkingHoursDto(this WorkingHours workingHours) => new(
        workingHours.UserId, workingHours.DateFrom, workingHours.DateTo, workingHours.Duration);
    
    public static WorkingHoursDto ToDto(this WorkingHours workingHours) => new(
        workingHours.UserId, workingHours.DateFrom, workingHours.DateTo, workingHours.Duration, workingHours.Id);
}