namespace LeaveSystem.Shared.WorkingHours;

public class WorkingHoursDto
{
    public WorkingHoursDto(string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo, TimeSpan duration, Guid id)
    {
        this.UserId = userId;
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
        this.Duration = duration;
        Id = id;
    }
    
    public WorkingHoursDto()
    {
    }

    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTimeOffset DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public TimeSpan Duration { get; set; }

    public string DurationProxy
    {
        get => Duration.ToString() ?? "";
        set
        {
            TimeSpan.TryParse(value, out var parsedValue);
            Duration = parsedValue;
        }
    }
}