namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequestDto
{
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public TimeSpan? Duration { get; set; }
    public Guid? LeaveTypeId { get; set; }
    public string? Remarks { get; set; }
}
