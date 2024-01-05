using GoldenEye.Objects.General;
using LeaveSystem.Shared;
using System.Text.Json;

namespace LeaveSystem.Db.Entities;
public class Setting : IHaveId<string>, IDisposable
{
    public string Id { get; set; } = null!;
    public SettingCategoryType Category { get; set; }
    public JsonDocument Value { get; set; } = null!;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Value?.Dispose();
        }
    }
}
