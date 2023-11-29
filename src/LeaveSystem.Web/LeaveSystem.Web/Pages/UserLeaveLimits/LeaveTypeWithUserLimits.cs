using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.LeaveTypes.GettingLeaveTypes;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public record LeaveTypeWithUserLimits(
    Guid Id,
    string Name, 
    IEnumerable<UserLeaveLimitDto> Limits,
    LeaveTypesService.LeaveTypeProperties Properties)
{
    public TimeSpan Limit => Limits.Aggregate(TimeSpan.Zero, (acc, x) => x.Limit + acc);
    public TimeSpan OverdueLimit => Limits.Aggregate(TimeSpan.Zero, (acc, x) => x.Limit + acc);
    public TimeSpan TotalLimit => Limits.Aggregate(TimeSpan.Zero, (acc, x) => x.Limit + acc);
    public static LeaveTypeWithUserLimits Create(
        Guid id,
        string name,
        IEnumerable<UserLeaveLimitDto> limits,
        LeaveTypesService.LeaveTypeProperties properties)
    {
        var limitsForType = limits.Where(x => x.LeaveTypeId == id);
        return new LeaveTypeWithUserLimits(id, name, limitsForType, properties);
    }
}

