using GoldenEye.Objects.General;

namespace LeaveSystem.Db.Entities;

public class WorkingHours : IHaveId<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset DateTo { get; set; }
    public TimeSpan Duration { get; set; }
}