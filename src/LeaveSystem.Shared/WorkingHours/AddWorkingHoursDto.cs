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

    public string? UserId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public TimeSpan? Duration { get; set; }
    public string DurationProxy
    {
        get => Duration.ToString() ?? "";
        set => Duration = TimeSpan.TryParse(value, out var parsedValue) ? parsedValue : TimeSpan.Zero;
    }
}