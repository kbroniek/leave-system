namespace LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;

using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

public class GetWorkingHoursQuery
{
    public static readonly WorkingHoursStatus[] ValidStatuses = { WorkingHoursStatus.Current };

    public GetWorkingHoursQuery(int pageNumber, int pageSize, DateTimeOffset dateFrom, DateTimeOffset dateTo,
        string[]? userIds, WorkingHoursStatus[]? statuses)
    {
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
        this.UserIds = userIds;
        this.Statuses = statuses;
    }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset DateTo { get; set; }
    public string[]? UserIds { get; set; }
    public WorkingHoursStatus[]? Statuses { get; set; }

    public static GetWorkingHoursQuery GetDefault()
    {
        var now = DateTimeOffset.UtcNow.GetDayWithoutTime();
        return new GetWorkingHoursQuery(
            1,
            100,
            now.AddDays(-14),
            now.AddDays(14),
            null,
            ValidStatuses);
    }

    public static GetWorkingHoursQuery GetDefaultForUsers(string[] userIds)
    {
        var now = DateTimeOffset.UtcNow.GetDayWithoutTime();
        return new GetWorkingHoursQuery(
            1,
            100,
            now.AddDays(-14),
            now.AddDays(14),
            userIds,
            ValidStatuses);
    }

    public static GetWorkingHoursQuery GetAllForUsers(string[] userIds, WorkingHoursStatus[] statuses,
        DateTimeOffset now) =>
        new(
            1,
            100,
            now.AddYears(-100), // TODO: provide null
            now.AddYears(100), // TODO: provide null
            userIds,
            statuses);
}
