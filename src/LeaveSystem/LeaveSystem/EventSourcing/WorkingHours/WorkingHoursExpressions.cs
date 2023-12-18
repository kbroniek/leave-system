using System.Linq.Expressions;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.EventSourcing.WorkingHours;

public static class WorkingHoursExpression
{
    public static Expression<Func<WorkingHours, bool>> GetExpressionForStatus(WorkingHoursStatus status, DateTimeOffset currentDate) =>
        status switch
        {
            WorkingHoursStatus.Deprecated => x => currentDate > x.DateTo,
            WorkingHoursStatus.Future => x => currentDate < x.DateFrom,
            WorkingHoursStatus.Current => x => (currentDate <= x.DateTo || x.DateTo == null) && currentDate >= x.DateFrom,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}