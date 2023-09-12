using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;

public class GetWorkingHoursQuery
{
    public static readonly WorkingHoursStatus[] ValidStatuses = { WorkingHoursStatus.Current };
    public int PageNumber { get; set; }
    public int PageSize { get; set;}
    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset DateTo { get; set; }
    public string[]? UserIds { get; set; }
    public WorkingHoursStatus[]? Statuses { get; set; }

    public GetWorkingHoursQuery(int pageNumber, int pageSize, DateTimeOffset dateFrom, DateTimeOffset dateTo, string[]? userIds, WorkingHoursStatus[]? statuses)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        DateFrom = dateFrom;
        DateTo = dateTo;
        UserIds = userIds;
        Statuses = statuses;
    }

    public static GetWorkingHoursQuery GetCurrentForUsers(int pageNumber, int pageSize, DateTimeOffset dateFrom,
        DateTimeOffset dateTo, string[]? userIds) =>
        new(pageNumber, pageSize, dateFrom, dateTo, userIds, ValidStatuses);


    public static GetWorkingHoursQuery GetDefault()
    {
        var now = DateTimeOffset.UtcNow.GetDayWithoutTime();
        return new(
            pageNumber: 1,
            pageSize: 100,
            dateFrom: now.AddDays(-14),
            dateTo: now.AddDays(14),
            userIds: null,
            statuses: ValidStatuses);
    }

    public static GetWorkingHoursQuery GetDefaultForUsers(string[] userIds)
    {
        var now = DateTimeOffset.UtcNow.GetDayWithoutTime();
        return new(
            pageNumber: 1,
            pageSize: 100,
            dateFrom: now.AddDays(-14),
            dateTo: now.AddDays(14),
            userIds: userIds,
            statuses: ValidStatuses);
    }
}