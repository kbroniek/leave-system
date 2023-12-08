using System.Text.Json.Serialization;
namespace LeaveSystem.Shared.UserLeaveLimits;

public class AddUserLeaveLimitDto
{
    public TimeSpan? Limit { get; set; }
    public TimeSpan? OverdueLimit { get; set; }
    public string? AssignedToUserId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public AddUserLeaveLimitPropertiesDto? Property { get; set; }
}

public class AddUserLeaveLimitPropertiesDto
{
    public string? Description { get; set; }
}