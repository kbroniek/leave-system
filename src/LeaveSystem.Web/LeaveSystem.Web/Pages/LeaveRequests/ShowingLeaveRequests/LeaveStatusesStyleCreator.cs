using System.Drawing;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public static class LeaveStatusesStyleCreator
{
    public static string Create(IEnumerable<GetLeaveStatusSettingsService.Setting> leaveStatusSettings)
    {
        var result = leaveStatusSettings
            .Where(ls => ls.Value.Color is not null)
            .Select(ls => CreateStyle(ls.Id, ls.Value.Color!));
        return string.Join(Environment.NewLine, result);
    }

    private static string CreateStyle(string status, string colorRaw)
    {
        if (string.Equals(colorRaw, "transparent", StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }
        var color = ConvertColor(colorRaw);

        return $@".vis-timeline .vis-item.leave-status-{status.ToLowerInvariant()} {{
    --label-status-h: {color.GetHue()};
    --label-status-s: {color.GetSaturation() * 100};
    --label-status-l: {color.GetBrightness() * 100};
}}";
    }
    public static Color ConvertColor(string color)
    {
        try { return ColorTranslator.FromHtml(color); }
        catch { return Color.Empty; }
    }
}
