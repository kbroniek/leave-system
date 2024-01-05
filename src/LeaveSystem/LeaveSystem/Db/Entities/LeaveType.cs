using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.Shared;

namespace LeaveSystem.Db.Entities;

public class LeaveType : IHaveId<Guid>
{
    object IHaveId.Id => this.Id;
    public Guid Id { get; set; }
    public Guid? BaseLeaveTypeId { get; set; }
    public string Name { get; set; } = null!;
    public LeaveTypeProperties? Properties { get; set; }
    public virtual LeaveType? BaseLeaveType { get; set; }
    public virtual ICollection<LeaveType>? ConstraintedLeaveTypes { get; set; }
    public virtual ICollection<UserLeaveLimit>? UserLeaveLimits { get; set; }
    public int Order { get; set; }
    public class LeaveTypeProperties
    {
        public string? Color { get; set; }
        public bool? IncludeFreeDays { get; set; }
        public TimeSpan? DefaultLimit { get; set; }
        public LeaveTypeCatalog? Catalog { get; set; }
    }
}
