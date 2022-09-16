namespace LeaveSystem.Shared;
public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset GetDayWithoutTime(this DateTimeOffset date) =>
        new DateTimeOffset(date.Ticks - (date.Ticks % TimeSpan.TicksPerDay), TimeSpan.Zero);
    public static DateTimeOffset GetFirstDayOfYear(this DateTimeOffset date) =>
        new DateTimeOffset(date.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static DateTimeOffset GetLastDayOfYear(this DateTimeOffset date) =>
        new DateTimeOffset(date.Year, 12, 31, 23, 59, 59, 999, TimeSpan.Zero);
}
