using System.Linq.Expressions;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.EventSourcing.WorkingHours;

public static class WorkingHoursStatusExpression
{
    private static WorkingHoursStatus GetStatus(this WorkingHours source, DateTimeOffset currentDate)
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

    public static Expression<Func<WorkingHours, bool>> GetExpressionForStatus(WorkingHoursStatus status, DateTimeOffset currentDate) =>
        status switch
        {
            WorkingHoursStatus.Deprecated => x => currentDate > x.DateTo,
            WorkingHoursStatus.Future => x => currentDate < x.DateFrom,
            WorkingHoursStatus.Current => x => currentDate < x.DateTo && currentDate > x.DateFrom,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}