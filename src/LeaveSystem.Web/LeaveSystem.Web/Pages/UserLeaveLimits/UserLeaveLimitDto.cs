using System.Text.Json.Serialization;
using LeaveSystem.Web.Extensions;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class UserLeaveLimitDto
{
    public UserLeaveLimitDto(Guid id, TimeSpan limit, TimeSpan overdueLimit, Guid leaveTypeId, DateTimeOffset? validSince, DateTimeOffset? validUntil, UserLeaveLimitPropertyDto property)
    {
        Limit = limit;
        OverdueLimit = overdueLimit;
        LeaveTypeId = leaveTypeId;
        ValidSince = validSince;
        ValidUntil = validUntil;
        Property = property;
        Id = id;
    }

    public UserLeaveLimitDto(){}

    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonIgnore]
    public TimeSpan TotalLimit => Limit + OverdueLimit;
    public TimeSpan Limit { get; set; }
    public TimeSpan OverdueLimit { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public UserLeaveLimitPropertyDto Property { get; set; } = new();

    public static UserLeaveLimitDto Create(LeaveLimitDto limit) =>
        new(limit.Id, limit.Limit, limit.OverdueLimit, limit.LeaveTypeId, limit.ValidSince, limit.ValidUntil, limit.Property ?? new ());
    private UserLeaveLimitDto(Guid leaveTypeId)
    {
        LeaveTypeId = leaveTypeId;
        
    }
    public static UserLeaveLimitDto Create(Guid leaveTypeId) => new(leaveTypeId);
}

public record LeaveLimitDto(Guid Id, TimeSpan Limit, TimeSpan OverdueLimit, Guid LeaveTypeId, DateTimeOffset? ValidSince, DateTimeOffset? ValidUntil, UserLeaveLimitPropertyDto? Property, string AssignedToUserId)
{
    public TimeSpan TotalLimit { get => Limit + OverdueLimit; }
}

public class UserLeaveLimitPropertyDto
{
    public UserLeaveLimitPropertyDto(string? Description)
    {
        this.Description = Description;
    }
    public UserLeaveLimitPropertyDto(){}

    public string? Description { get; set; }
}