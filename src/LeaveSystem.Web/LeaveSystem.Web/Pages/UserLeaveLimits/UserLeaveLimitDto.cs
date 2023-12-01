using LeaveSystem.Web.Extensions;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class UserLeaveLimitDto
{
    private UserLeaveLimitDto(TimeSpan Limit, TimeSpan OverdueLimit, Guid LeaveTypeId, DateTimeOffset? ValidSince, DateTimeOffset? ValidUntil, UserLeaveLimitPropertyDto? Property)
    {
        this.Limit = Limit;
        this.OverdueLimit = OverdueLimit;
        this.LeaveTypeId = LeaveTypeId;
        this.ValidSince = ValidSince;
        this.ValidUntil = ValidUntil;
        this.Property = Property;
    }

    public TimeSpan TotalLimit => Limit + OverdueLimit;
    public TimeSpan Limit { get; set; }
    public TimeSpan OverdueLimit { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public UserLeaveLimitPropertyDto? Property { get; set; }

    public static UserLeaveLimitDto Create(LeaveLimitDto limit) =>
        new(limit.Limit, limit.OverdueLimit, limit.LeaveTypeId, limit.ValidSince, limit.ValidUntil, limit.Property);
    private UserLeaveLimitDto(){}
    public static UserLeaveLimitDto CreateEmpty() => new();
}

public record LeaveLimitDto(TimeSpan Limit, TimeSpan OverdueLimit, Guid LeaveTypeId, DateTimeOffset? ValidSince, DateTimeOffset? ValidUntil, UserLeaveLimitPropertyDto? Property, string AssignedToUserId)
{
    public TimeSpan TotalLimit { get => Limit + OverdueLimit; }
}

public record UserLeaveLimitPropertyDto(string? Description);