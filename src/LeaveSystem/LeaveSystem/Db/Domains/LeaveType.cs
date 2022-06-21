using GoldenEye.Objects.General;
using System.Drawing;

namespace LeaveSystem.Api.Domains;

public class LeaveType : IHaveId<Guid>
{
    public Guid LeaveTypeId { get; set; }
    public Guid? BaseLeaveTypeId { get; set; }
    public string Title { get; set; } = "";
    public LeaveTypeProperties? Properties { get; set; }
    public LeaveType? BaseLeaveType { get; set; }
    public ICollection<LeaveType>? ConstraintedLeaveTypes { get; set; }
    public Guid Id => LeaveTypeId;
}

public class LeaveTypeProperties
{
    public string? Color { get; set; }
}
