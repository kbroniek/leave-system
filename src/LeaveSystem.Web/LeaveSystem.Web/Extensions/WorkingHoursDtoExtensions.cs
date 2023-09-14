using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Extensions;

public static class WorkingHoursDtoExtensions
{
    public static TimeSpan DurationOrDefault(this WorkingHoursDto? source) =>
        source?.Duration ?? TimeSpan.Zero;

    public static TimeSpan DurationOrDefault(this IEnumerable<WorkingHoursDto?>? source, string userId) =>
        (source?.FirstOrDefault(wh => wh?.UserId == userId)).DurationOrDefault();
}