using LeaveSystem.Shared;
using System.Text;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;


public static class HolidayStyleCreator
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
                cssClasses.Add($".vis-time-axis .vis-grid.vis-day{currentDate.Day}.vis-{currentDate.ToString("MMMM").ToLower()}");
            }
        }
        var result = cssClasses.Count > 0 ? $@"{string.Join($",{Environment.NewLine}", cssClasses)}
{{
    background-color: #ffe0c9
}}" : "";
        Console.WriteLine(result);
        return result;
    }
}