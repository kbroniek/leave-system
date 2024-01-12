using System.Text.Json;
using FluentValidation;
using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.Shared;

namespace LeaveSystem.Db.Entities;
public class Setting : IHaveId<string>, IDisposable
{
    object IHaveId.Id => this.Id;
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

public class SettingValidator : AbstractValidator<Setting>
{
    public SettingValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
    }
}
