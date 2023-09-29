namespace LeaveSystem.Shared.WorkingHours;

public class AddWorkingHoursDto
{
    public AddWorkingHoursDto(string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo, TimeSpan duration)
    {
        UserId = userId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
    }

    public AddWorkingHoursDto()
    {
    }
    
    public AddWorkingHoursDto(string userId)
    {
        UserId = userId;
    }

    public string? UserId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public TimeSpan? Duration { get; set; }
    public DateTime? DurationAsDateTime
    {
        get => new(Duration?.Ticks ?? 0);
        set => Duration = TimeSpan.FromTicks(value?.Ticks ?? 0);
    }
}