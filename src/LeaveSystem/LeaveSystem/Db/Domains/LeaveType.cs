﻿using GoldenEye.Objects.General;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveSystem.Api.Domains;

public class LeaveType : IHaveId<Guid>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid LeaveTypeId { get; set; }
    [Required]
    public string? Description { get; set; }
    public string? Properties { get; set; }

    [NotMapped]
    public Guid Id => LeaveTypeId;
}

