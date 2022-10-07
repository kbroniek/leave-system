using LeaveSystem.EventSourcing.LeaveRequests;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequest;

public class GetLeaveRequestsQuery
{
    public GetLeaveRequestsQuery()
    {
        var now = DateTimeOffset.UtcNow;
        DateFrom = now.AddDays(-14);
        DateTo = now.AddDays(14);
        PageNumber = 0;
        PageSize = 100;
        Statuses = new[] { LeaveRequestStatus.Accepted, LeaveRequestStatus.Pending };
    }

    public string[]? CreatedByEmails { get; set; }
    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset DateTo { get; set; }
    public Guid[]? LeaveTypeIds { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public LeaveRequestStatus[] Statuses { get; set; }
}
