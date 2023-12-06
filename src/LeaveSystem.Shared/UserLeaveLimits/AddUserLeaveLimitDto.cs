namespace LeaveSystem.Shared.UserLeaveLimits;

public class AddUserLeaveLimitDto
{
    public TimeSpan? Limit { get; set; }
    public TimeSpan? OverdueLimit { get; set; }
    public string? AssignedToUserId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int Year { get; set; }
    public string? Description { get; set; }
}