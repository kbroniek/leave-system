using System.Text.Json.Serialization;
namespace LeaveSystem.Shared.UserLeaveLimits;

public class AddUserLeaveLimitDto
{
    // [JsonPropertyName("@odata.type")]
    // public string OdataType => "LeaveSystem.Db.Entities.UserLeaveLimit";
    public Guid Id { get; set; } = Guid.NewGuid();
    public TimeSpan? Limit { get; set; }
    public TimeSpan? OverdueLimit { get; set; }
    public string? AssignedToUserId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTimeOffset? ValidSince { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public TestProperties? Properties { get; set; }
 
}

public class TestProperties
{
    public string? Description { get; set; }
}