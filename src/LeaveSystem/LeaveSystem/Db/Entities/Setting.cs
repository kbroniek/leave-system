using GoldenEye.Objects.General;
using System.Text.Json;

namespace LeaveSystem.Db.Entities;
public class Setting : IHaveId<string>, IDisposable
{
    public string Id { get; set; }
    public CategoryType Category { get; set; }
    public JsonDocument Value { get; set; }
    public void Dispose() => Value?.Dispose();

    public enum CategoryType
    {
        LeaveStatus
    }
}
