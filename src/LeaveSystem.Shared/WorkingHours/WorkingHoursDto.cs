namespace LeaveSystem.Shared.WorkingHours;

public class WorkingHoursDto
{
    public WorkingHoursDto(string UserId, DateTimeOffset DateFrom, DateTimeOffset? DateTo, TimeSpan Duration, Guid id)
    {
        this.UserId = UserId;
        this.DateFrom = DateFrom;
        this.DateTo = DateTo;
        this.Duration = Duration;
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

    public DateTime DurationAsDateTime
    {
        get => new(Duration.Ticks);
        set => Duration = TimeSpan.FromTicks(value.Ticks);
    }
}