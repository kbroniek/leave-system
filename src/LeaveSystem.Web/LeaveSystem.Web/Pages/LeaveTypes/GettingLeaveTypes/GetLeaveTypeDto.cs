namespace LeaveSystem.Web.Pages.LeaveTypes.GettingLeaveTypes;

public class GetLeaveTypeDto
{
    public Guid LeaveTypeId { get; set; }
    public Guid? BaseLeaveTypeId { get; set; }
    public string? Name{ get; set; }
    public LeaveTypePropertiesDto? Properties { get; set; }
    public Guid Id => LeaveTypeId;
    public class LeaveTypePropertiesDto
    {
        public string? Color { get; set; }
        public bool? IncludeFreeDays { get; set; }
        public TimeSpan? DefaultLimit { get; set; }
    }
}

