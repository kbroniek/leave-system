using GoldenEye.Objects.General;
using LeaveSystem.Shared;
using System.Text.Json;

namespace LeaveSystem.Db.Entities;
public class Setting : IHaveId<string>, IDisposable
{
    public string Id { get; set; }
    public SettingCategoryType Category { get; set; }
    public JsonDocument Value { get; set; }
    public void Dispose() => Value?.Dispose();
}
