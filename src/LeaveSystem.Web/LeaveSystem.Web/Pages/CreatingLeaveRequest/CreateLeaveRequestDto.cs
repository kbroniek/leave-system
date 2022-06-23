namespace LeaveSystem.Web.Pages.CreatingLeaveRequest;

public class CreateLeaveRequestDto
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int? Hours { get; set; }
    public Guid? Type { get; set; }
    public string? Remarks { get; set; }
}
