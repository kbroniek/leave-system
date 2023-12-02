using LeaveSystem.Db.Entities;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class UserLeaveLimitsExtensions
{
    public static UserLeaveLimitDto ToDto(this UserLeaveLimit leaveLimit) =>
        new(leaveLimit.Id, leaveLimit.Limit ?? TimeSpan.Zero, leaveLimit.OverdueLimit ?? TimeSpan.Zero, leaveLimit.LeaveTypeId, leaveLimit.ValidSince,
            leaveLimit.ValidUntil, leaveLimit.Property?.ToDto());

    public static UserLeaveLimitPropertyDto ToDto(this UserLeaveLimit.UserLeaveLimitProperties property)
        => new(property.Description);
}