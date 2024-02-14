namespace LeaveSystem.Web.Pages.UserLeaveLimits;

using System.Text.Json.Serialization;

public class UserLeaveLimitDto
{
    [JsonConstructor]
    public UserLeaveLimitDto(Guid id, TimeSpan limit, TimeSpan overdueLimit, Guid leaveTypeId,
        DateTimeOffset? validSince, DateTimeOffset? validUntil, UserLeaveLimitPropertyDto? property)
    {
        this.Limit = limit;
        this.OverdueLimit = overdueLimit;
        this.LeaveTypeId = leaveTypeId;
        this.ValidSince = validSince;
        this.ValidUntil = validUntil;
        this.Property = property ?? new();
        this.Id = id;
    }

    public UserLeaveLimitDto() { }

    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonIgnore] public TimeSpan TotalLimit => this.Limit + this.OverdueLimit;

    public TimeSpan Limit { get; set; }
    public TimeSpan OverdueLimit { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public UserLeaveLimitPropertyDto Property { get; set; } = new();

    public static UserLeaveLimitDto Create(LeaveLimitDto limit) =>
        new(limit.Id, limit.Limit, limit.OverdueLimit, limit.LeaveTypeId, limit.ValidSince, limit.ValidUntil,
            limit.Property ?? new UserLeaveLimitPropertyDto());
}

public record LeaveLimitDto(
    Guid Id,
    TimeSpan Limit,
    TimeSpan OverdueLimit,
    Guid LeaveTypeId,
    DateTimeOffset? ValidSince,
    DateTimeOffset? ValidUntil,
    UserLeaveLimitPropertyDto? Property,
    string AssignedToUserId)
{
    public TimeSpan TotalLimit => this.Limit + this.OverdueLimit;
}

public class UserLeaveLimitPropertyDto
{
    [JsonConstructor]
    public UserLeaveLimitPropertyDto(string? description) => this.Description = description ?? string.Empty;
    public UserLeaveLimitPropertyDto() { }

    public string Description { get; set; } = string.Empty;
}
