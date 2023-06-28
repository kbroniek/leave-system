using System.Drawing;
using static LeaveSystem.Web.Pages.LeaveTypes.LeaveTypesService;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public static class LeaveTypeStyleCreator
{
    public static string Create(IEnumerable<LeaveTypeDto> leaveTypes)
    {
        var result = leaveTypes
            .Where(lt => lt.Properties.Color is not null)
            .Select(lt => CreateStyle(lt.Id, lt.Properties.Color!));
        return string.Join(Environment.NewLine, result);
    }

    private static string CreateStyle(Guid id, string colorRaw)
    {
        var color = ConvertColor(colorRaw);

        return $@".vis-timeline .vis-item.leave-type-{id} {{
    --label-r: {color.R};
    --label-g: {color.G};
    --label-b: {color.B};
    --label-h: {color.GetHue()};
    --label-s: {color.GetSaturation() * 100};
    --label-l: {color.GetBrightness() * 100};
}}";
    }
    public static Color ConvertColor(string color)
    {
        try { return ColorTranslator.FromHtml(color); }
        catch { return Color.Empty; }
    }
}
