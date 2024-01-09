using System.Linq.Expressions;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.LeaveTypes.GettingLeaveTypes;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class LeaveTypeForLimitsView
{
    public string Limit => GetReadableSumTimespanFromTicks(x => x.Limit.Ticks);
    public string OverdueLimit => GetReadableSumTimespanFromTicks(x => x.OverdueLimit.Ticks);
    public string TotalLimit => GetReadableSumTimespanFromTicks(x => x.TotalLimit.Ticks);
    public Guid Id { get; init; }
    public string Name { get; init; }
    public List<UserLeaveLimitDto> Limits { get; init; }
    public LeaveTypesService.LeaveTypeProperties Properties { get; init; }
    private TimeSpan workingHours;

    public LeaveTypeForLimitsView(
        Guid id,
        string name, 
        List<UserLeaveLimitDto> limits,
        LeaveTypesService.LeaveTypeProperties properties,
        TimeSpan workingHours)
    {
        Id = id;
        Name = name;
        Limits = limits;
        Properties = properties;
        this.workingHours = workingHours;
    }

    public static LeaveTypeForLimitsView Create(
        Guid id,
        string name,
        IEnumerable<UserLeaveLimitDto> limits,
        LeaveTypesService.LeaveTypeProperties properties,
        TimeSpan workingHours)
    {
        var limitsForType = limits.Where(x => x.LeaveTypeId == id).ToList();
        return new LeaveTypeForLimitsView(id, name, limitsForType, properties, workingHours);
    }

    private string GetReadableSumTimespanFromTicks(Func<UserLeaveLimitDto, long> expression) 
        => TimeSpan.FromTicks(Limits.Sum(expression)).GetReadableTimeSpan(workingHours);
}

