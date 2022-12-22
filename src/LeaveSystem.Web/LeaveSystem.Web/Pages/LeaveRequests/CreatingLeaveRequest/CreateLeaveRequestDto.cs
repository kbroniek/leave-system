namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public record class CreateLeaveRequestDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public TimeSpan? Duration { get; set; }
    public Guid? LeaveTypeId { get; set; }
    public string? Remarks { get; set; }
}
