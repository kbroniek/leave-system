using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Linq;
using LeaveSystem.Shared.WorkingHours;
using Marten;

namespace LeaveSystem.Extensions;

public static class QuerySessionExtensions
{
    public static Task<WorkingHours?> GetCurrentWorkingHoursForUser(
        this IQuerySession querySession, string userId, DateTimeOffset currentDate, CancellationToken cancellationToken) => 
        querySession.Query<WorkingHours>()
            .Where(WorkingHoursExpression.GetExpressionForStatus(WorkingHoursStatus.Current, currentDate).And(x => x.UserId == userId))
            .FirstOrDefaultAsync(cancellationToken);
}