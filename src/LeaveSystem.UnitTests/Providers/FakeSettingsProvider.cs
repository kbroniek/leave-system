using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;

namespace LeaveSystem.UnitTests.Providers;

public class FakeSettingsProvider
{
    public static readonly string CanceledSettingId = LeaveRequestStatus.Canceled.ToString();
    public static readonly string RejectedSettingId = LeaveRequestStatus.Rejected.ToString();
    public static readonly string PendingSettingId = LeaveRequestStatus.Pending.ToString();
    public static readonly string AcceptedSettingId = LeaveRequestStatus.Accepted.ToString();

    public static Setting GetCanceledSetting() => new()
    {
        Id = CanceledSettingId, Category = SettingCategoryType.LeaveStatus,
        Value = JsonDocument.Parse("{\"color\": \"#525252\"}")
    };

    public static Setting GetRejectedSetting() => new()
    {
        Id = RejectedSettingId, Category = SettingCategoryType.LeaveStatus,
        Value = JsonDocument.Parse("{\"color\": \"#850000\"}")
    };

    public static Setting GetPendingSetting() => new()
    {
        Id = PendingSettingId, Category = SettingCategoryType.LeaveStatus,
        Value = JsonDocument.Parse("{\"color\": \"#CFFF98\"}")
    };

    public static Setting GetAcceptedSetting() => new()
    {
        Id = AcceptedSettingId, Category = SettingCategoryType.LeaveStatus,
        Value = JsonDocument.Parse("{\"color\": \"transparent\"}")
    };

    public static IQueryable<Setting> GetSettings()
    {
        return new List<Setting>
        {
            GetPendingSetting(),
            GetRejectedSetting(),
            GetAcceptedSetting(),
            GetCanceledSetting()
        }.AsQueryable();

    }
}