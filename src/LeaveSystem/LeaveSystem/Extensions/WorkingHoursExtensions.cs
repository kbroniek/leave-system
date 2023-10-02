using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Extensions;

public static class WorkingHoursExtensions
{
    public static WorkingHoursStatus GetStatus(this WorkingHours source, DateTimeOffset currentDate)
    {
        if (currentDate > source.DateTo)
        {
            return WorkingHoursStatus.Deprecated;
        }
        if (currentDate < source.DateFrom)
        {
            return WorkingHoursStatus.Future;
        }
        return WorkingHoursStatus.Current;
    }
}