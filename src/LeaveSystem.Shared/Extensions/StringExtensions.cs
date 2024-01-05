namespace LeaveSystem.Shared.Extensions;

public static class StringExtensions
{
    public static TimeSpan? ToTimeSpan(this string? value)
    {
        if (value == null)
        {
            return null;
        }
        return TimeSpan.TryParse(value, out var parsedValue) ? parsedValue : TimeSpan.Zero;
    }
}
