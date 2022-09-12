using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Entities;
public class Role : IHaveId<Guid>
{
    public Guid RoleId { get; set; }
    public Roles RoleName { get; set; }
    public string Email { get; set; }

    public Guid Id => RoleId;
}

public enum Roles
{
    Employee,
    LeaveLimitAdmin,
    DecisionMaker,
    HumanResource,
    GlobalAdmin
}

