using LeaveSystem.EventSourcing.LeaveRequests;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequest;

public class GetLeaveRequestsQuery
{
    public static readonly GetLeaveRequestsQuery Default = GetDefault();

    public GetLeaveRequestsQuery(DateTimeOffset dateFrom, DateTimeOffset dateTo, int pageNumber, int pageSize, LeaveRequestStatus[] statuses)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Statuses = statuses;
    }

    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset DateTo { get; set; }
    public Guid[]? LeaveTypeIds { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public LeaveRequestStatus[] Statuses { get; set; }
    public string[]? CreatedByEmails { get; set; }
    private static GetLeaveRequestsQuery GetDefault()
    {
        var now = DateTimeOffset.UtcNow;
        return new GetLeaveRequestsQuery
        (
            dateFrom: now.AddDays(-14),
            dateTo: now.AddDays(14),
            pageNumber: 0,
            pageSize: 100,
            statuses: new[] { LeaveRequestStatus.Accepted, LeaveRequestStatus.Pending }
        );
    }
}
