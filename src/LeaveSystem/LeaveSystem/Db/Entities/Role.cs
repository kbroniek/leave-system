using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Entities;
public class Role : IHaveId<Guid>
{
    public Guid Id { get; set; }
    public RoleType RoleType { get; set; }
    public string UserId { get; set; }
}

public enum RoleType
{
    Employee,
    LeaveLimitAdmin,
    DecisionMaker,
    HumanResource,
    GlobalAdmin
}

