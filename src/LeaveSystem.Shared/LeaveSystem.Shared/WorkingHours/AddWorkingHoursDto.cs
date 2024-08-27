namespace LeaveSystem.Shared.WorkingHours;

using System.Text.Json.Serialization;

public class AddWorkingHoursDto
{
    [JsonConstructor]
    public AddWorkingHoursDto(){}
    public AddWorkingHoursDto(string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo, TimeSpan duration)
    {
        this.UserId = userId;
        this.DateFrom = dateFrom;
        this.DateTo = dateTo;
        this.Duration = duration;
    }

    public string? UserId { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public TimeSpan? Duration { get; set; }
    [JsonIgnore]
    public string DurationProxy
    {
        get => Duration.ToString() ?? "";
        set => Duration = TimeSpan.TryParse(value, out var parsedValue) ? parsedValue : TimeSpan.Zero;
    }
}
