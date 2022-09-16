using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Entities;

public class LeaveType : IHaveId<Guid>
{
    public Guid Id { get; set; }
    public Guid? BaseLeaveTypeId { get; set; }
    public string Name { get; set; }
    public LeaveTypeProperties? Properties { get; set; }
    public LeaveType? BaseLeaveType { get; set; }
    public ICollection<LeaveType>? ConstraintedLeaveTypes { get; set; }
    public ICollection<UserLeaveLimit>? UserLeaveLimits { get; set; }
    public class LeaveTypeProperties
    {
        public string? Color { get; set; }
        public bool? IncludeFreeDays { get; set; }
        public TimeSpan? DefaultLimit { get; set; }
    }
}
