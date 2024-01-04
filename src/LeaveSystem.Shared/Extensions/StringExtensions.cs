namespace LeaveSystem.Shared.Extensions;

public static class StringExtensions
{
    public static TimeSpan? ToTimeSpan(this string? value)
    {
        if(value == null)
        {
            return null;
        }
        TimeSpan.TryParse(value, out var parsedValue);
        return parsedValue;
    }
}
