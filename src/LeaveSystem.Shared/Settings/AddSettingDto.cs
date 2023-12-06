using System.Text.Json;

namespace LeaveSystem.Shared.Settings;

public class AddSettingDto
{
    public string? Id { get; set; }
    public SettingCategoryType Category { get; set; }
    public JsonDocument? Value { get; set; }
}