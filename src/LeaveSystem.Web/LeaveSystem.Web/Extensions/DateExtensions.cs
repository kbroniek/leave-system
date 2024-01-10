namespace LeaveSystem.Web.Extensions;

using System.Globalization;

public static class DateExtensions
{
    private static readonly CultureInfo Pl = new("pl-PL");
    public static string GetReadableDate(this DateTimeOffset? date) =>
        date.HasValue ? date.Value.ToString("d", Pl) : "---";
}
