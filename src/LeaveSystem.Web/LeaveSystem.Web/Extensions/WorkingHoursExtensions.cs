using LeaveSystem.EventSourcing.WorkingHours;

namespace LeaveSystem.Web.Extensions;

public static class WorkingHoursExtensions
{
    public static TimeSpan DurationOrDefault(this WorkingHours? source) =>
        source?.Duration ?? TimeSpan.Zero;

    public static TimeSpan DurationOrDefault(this IEnumerable<WorkingHours?>? source, string userId) =>
        (source?.FirstOrDefault(wh => wh?.UserId == userId)).DurationOrDefault();
}