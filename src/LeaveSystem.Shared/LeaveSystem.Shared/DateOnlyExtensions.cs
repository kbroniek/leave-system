namespace LeaveSystem.Shared;
public static class DateOnlyExtensions
{
    public static DateOnly GetFirstDayOfYear(this DateOnly date) =>
        GetFirstDayOfYear(date.Year);
    public static DateOnly GetFirstDayOfYear(int year) =>
        new(year, 1, 1);
    public static DateOnly GetLastDayOfYear(this DateOnly date) =>
        GetLastDayOfYear(date.Year);
    public static DateOnly GetLastDayOfYear(int year) =>
        new(year, 12, 31);
}
