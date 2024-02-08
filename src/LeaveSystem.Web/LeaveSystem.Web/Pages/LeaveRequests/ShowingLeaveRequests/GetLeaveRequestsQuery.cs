namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;

public class GetLeaveRequestsQuery
{
    private static readonly LeaveRequestStatus[] ValidStatuses =
    {
        LeaveRequestStatus.Accepted, LeaveRequestStatus.Pending
    };

    public GetLeaveRequestsQuery(DateTimeOffset dateFrom, DateTimeOffset dateTo, int pageNumber, int pageSize)
    {
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.Statuses = ValidStatuses;
    }

    public GetLeaveRequestsQuery(DateTimeOffset dateFrom, DateTimeOffset dateTo, int pageNumber, int pageSize,
        IEnumerable<LeaveRequestStatus> statuses)
    {
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        this.Statuses = statuses;
    }

    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset DateTo { get; set; }
    public IEnumerable<Guid> LeaveTypeIds { get; set; } = Enumerable.Empty<Guid>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<LeaveRequestStatus> Statuses { get; set; }
    public string[]? CreatedByEmails { get; set; }
    public string[]? CreatedByUserIds { get; set; }

    public static GetLeaveRequestsQuery GetDefault()
    {
        var now = DateTimeOffset.UtcNow.GetDayWithoutTime();
        return new GetLeaveRequestsQuery
        (
            now.AddDays(-14),
            now.AddDays(14),
            1,
            100,
            ValidStatuses
        );
    }
}
