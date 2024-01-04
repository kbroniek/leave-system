using System.Globalization;
using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;


public static class HolidaysStyleCreator
{
    public static string Create(DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        var endDate = dateTo.GetDayWithoutTime();
        var cssClasses = new List<string>();
        for (var currentDate = dateFrom.GetDayWithoutTime(); currentDate <= endDate; currentDate = currentDate.AddDays(1))
        {
            if (currentDate.DayOfWeek != DayOfWeek.Saturday
                && currentDate.DayOfWeek != DayOfWeek.Sunday
                && DateCalculator.GetDayKind(currentDate) == DateCalculator.DayKind.HOLIDAY)
            {
                cssClasses.Add($".vis-time-axis .vis-grid.vis-day{currentDate.Day}.vis-{currentDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("en-US")).ToLowerInvariant()}");
            }
        }
        // TODO: Get color from setting
        var result = cssClasses.Count > 0 ? $@"{string.Join($",{Environment.NewLine}", cssClasses)}
{{
    background-color: #ffe0c9
}}" : "";
        return result;
    }
}