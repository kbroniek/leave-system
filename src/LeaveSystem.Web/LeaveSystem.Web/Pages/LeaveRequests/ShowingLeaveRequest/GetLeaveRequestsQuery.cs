using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequest;

public class GetLeaveRequestsQuery
{
    public GetLeaveRequestsQuery(DateTimeOffset dateFrom, DateTimeOffset dateTo, int pageNumber, int pageSize, IEnumerable<LeaveRequestStatus> statuses)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Statuses = statuses;
    }

    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset DateTo { get; set; }
    public IEnumerable<Guid> LeaveTypeIds { get; set; } = Enumerable.Empty<Guid>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<LeaveRequestStatus> Statuses { get; set; }
    public string[]? CreatedByEmails { get; set; }
    public static GetLeaveRequestsQuery GetDefault()
    {
        var now = DateTimeOffset.UtcNow.GetDayWithoutTime();
        return new GetLeaveRequestsQuery
        (
            dateFrom: now.AddDays(-14),
            dateTo: now.AddDays(14),
            pageNumber: 1,
            pageSize: 100,
            statuses: new[] { LeaveRequestStatus.Accepted, LeaveRequestStatus.Pending }
        );
    }
}
