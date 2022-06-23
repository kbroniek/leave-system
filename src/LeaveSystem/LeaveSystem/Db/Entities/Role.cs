using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Entities;
public class Role : IHaveId<Guid>
{
    public Guid RoleId { get; set; }
    public string? Name { get; set; }
    public FederatedUser? User { get; set; }

    public Guid Id => RoleId;
}
