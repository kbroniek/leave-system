namespace LeaveSystem.Web.Extensions;

public static class DateExtensions
{
    public static string GetReadableDate(this DateTimeOffset? date) =>
        date.HasValue ? date.Value.ToString("d", Thread.CurrentThread.CurrentCulture) : "---";
}
