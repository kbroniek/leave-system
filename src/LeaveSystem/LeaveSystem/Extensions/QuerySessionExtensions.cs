using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Shared.WorkingHours;
using Marten;

namespace LeaveSystem.Extensions;

public static class QuerySessionExtensions
{
    public static Task<WorkingHours?> GetCurrentWorkingHoursForUser(
        this IQuerySession querySession, string userId, DateTimeOffset currentDate, CancellationToken cancellationToken) => 
        querySession.Query<WorkingHours>()
            .Where(x => x.UserId == userId && x.GetStatus(currentDate) == WorkingHoursStatus.Current)
            .FirstOrDefaultAsync(cancellationToken);
}