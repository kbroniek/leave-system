using GoldenEye.Objects.General;
using LeaveSystem.Shared;

namespace LeaveSystem.Db.Entities;
public class UserLeaveLimit : IHaveId<Guid>
{
    private DateTimeOffset? validSince;
    private DateTimeOffset? validUntil;
    public Guid Id { get; set; }
    public TimeSpan? Limit { get; set; }
    public TimeSpan? OverdueLimit { get; set; }
    public string? AssignedToUserId { get; set; }
    public virtual LeaveType LeaveType { get; set; }
    public Guid LeaveTypeId { get; set; }

    public DateTimeOffset? ValidSince
    {
        get => validSince;
        set => validSince = value?.GetDayWithoutTime();
    }

    public DateTimeOffset? ValidUntil
    {
        get => validUntil;
        set => validUntil = value?.GetDayWithoutTime();
    }

    public UserLeaveLimitProperties? Property { get; set; }

    public class UserLeaveLimitProperties
    {
        public string? Description { get; set; }
    }
}
