using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveSystem.Api.Domains;

public class LeaveType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid LeaveTypeId { get; set; }
    [Required]
    public string? Description { get; set; }
    [Required]
    public string? Abbreviation { get; set; }
    public string? Properties { get; set; }
}

