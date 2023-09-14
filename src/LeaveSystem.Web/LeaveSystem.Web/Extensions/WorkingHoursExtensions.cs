using LeaveSystem.Web.Pages.WorkingHours;

namespace LeaveSystem.Web.Extensions;

public static class WorkingHoursExtensions
{
    public static TimeSpan DurationOrDefault(this WorkingHoursDto? source) =>
        source?.Duration ?? TimeSpan.Zero;

    public static TimeSpan DurationOrDefault(this IEnumerable<WorkingHoursDto?>? source, string userId) =>
        (source?.FirstOrDefault(wh => wh?.UserId == userId)).DurationOrDefault();
}