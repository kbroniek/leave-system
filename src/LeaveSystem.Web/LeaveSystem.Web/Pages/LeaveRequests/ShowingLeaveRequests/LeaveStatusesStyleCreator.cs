using LeaveSystem.Shared.LeaveRequests;
using System.Drawing;
using static LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests.ShowLeaveRequests;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public static class LeaveStatusesStyleCreator
{
    public static string Create(IEnumerable<LeaveRequestStatusProperty> leaveStatuses)
    {
        var result = leaveStatuses
            .Where(ls => ls.Color is not null)
            .Select(ls => CreateStyle(ls.Status, ls.Color!));
        return string.Join(Environment.NewLine, result);
    }

    private static string CreateStyle(LeaveRequestStatus status, string colorRaw)
    {
        if(string.Equals(colorRaw, "transparent", StringComparison.OrdinalIgnoreCase))
        {
            return "";
        }
        var color = ConvertColor(colorRaw);

        return $@".vis-timeline .vis-item.leave-status-{status.ToString().ToLower()} {{
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
