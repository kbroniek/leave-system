using System.ComponentModel.DataAnnotations;

namespace LeaveSystem.Web.Pages.AddLeaveRequest;

public class LeaveRequestModel
{
    [Required]
    public DateTime DateFrom { get; set; }
    [Required]
    public DateTime DateTo { get; set; }

    [Range(0, 100000, ErrorMessage = "Hours invalid (0-100000).")]
    public int? Hours { get; set; }

    [Required]
    public Guid? Type { get; set; }

    public string? Remarks { get; set; }
}
