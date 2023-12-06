namespace LeaveSystem.Shared.LeaveRequests;

public class AddLeaveTypeDto
{
    public Guid Id { get; set; }
    public Guid? BaseLeaveTypeId { get; set; }
    public string? Name { get; set; }
    public int Order { get; set; }
    public string? Color { get; set; }
    public bool? IncludeFreeDays { get; set; }
    public TimeSpan? DefaultLimit { get; set; }
    public LeaveTypeCatalog? Catalog { get; set; }
}