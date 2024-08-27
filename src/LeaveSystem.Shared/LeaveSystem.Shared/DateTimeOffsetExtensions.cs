namespace LeaveSystem.Shared;
public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset GetDayWithoutTime(this DateTimeOffset date) =>
        new DateTimeOffset(date.Ticks - (date.Ticks % TimeSpan.TicksPerDay), TimeSpan.Zero);
    public static DateTimeOffset GetFirstDayOfYear(this DateTimeOffset date) =>
        GetFirstDayOfYear(date.Year);
    public static DateTimeOffset GetLastDayOfYear(this DateTimeOffset date) =>
        GetLastDayOfYear(date.Year);
    public static DateTimeOffset GetFirstDayOfYear(int year) =>
        new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static DateTimeOffset GetLastDayOfYear(int year) =>
        new DateTimeOffset(year, 12, 31, 23, 59, 59, 999, TimeSpan.Zero);
    public static DateTimeOffset CreateFromDate(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, TimeSpan.Zero);
}
