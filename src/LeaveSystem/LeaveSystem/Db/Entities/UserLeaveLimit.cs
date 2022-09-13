using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Entities;
public class UserLeaveLimit : IHaveId<Guid>
{
    public Guid UserLeaveLimitId { get; set; }
    public TimeSpan? Limit { get; set; }
    public TimeSpan? OverdueLimit { get; set; }
    public string? AssignedToUserEmail { get; set; }
    public LeaveType LeaveType { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public UserLeaveLimitProperties? Property { get; set; }
    public Guid Id => UserLeaveLimitId;

    public class UserLeaveLimitProperties
    {
        public string? Description { get; set; }
    }
}
