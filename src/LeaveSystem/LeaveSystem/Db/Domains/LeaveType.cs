﻿using GoldenEye.Objects.General;
using System.Drawing;

namespace LeaveSystem.Db.Domains;

public class LeaveType : IHaveId<Guid>
{
    public Guid LeaveTypeId { get; set; }
    public Guid? BaseLeaveTypeId { get; set; }
    public string Title { get; set; } = "";
    public LeaveTypeProperties? Properties { get; set; }
    public LeaveType? BaseLeaveType { get; set; }
    public ICollection<LeaveType>? ConstraintedLeaveTypes { get; set; }
    public ICollection<UserLeaveLimit>? UserLeaveLimits { get; set; }
    public Guid Id => LeaveTypeId;
    public class LeaveTypeProperties
    {
        public string? Color { get; set; }
        public bool IncludeFreeDays { get; set; }
        public int? DefaultLimit { get; set; }
    }
}
