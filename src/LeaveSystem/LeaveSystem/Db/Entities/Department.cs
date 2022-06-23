using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Entities;
public class Department : IHaveId<Guid>
{
    public Guid DepartmentId { get; set; }
    public string? Name { get; set; }
    public FederatedUser[]? Users { get; set; }

    public Guid Id => DepartmentId;
}
