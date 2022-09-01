namespace LeaveSystem.Shared;
public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset GetDayWithoutTime(this DateTimeOffset date) =>
        new DateTimeOffset(date.Ticks - (date.Ticks % TimeSpan.TicksPerDay), TimeSpan.Zero);
}
