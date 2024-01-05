namespace LeaveSystem.Shared.Date;

public class CustomDateService : DateService
{
    private readonly DateTimeOffset customDate;

    public CustomDateService(DateTimeOffset customDate)
    {
        this.customDate = customDate;
    }

    public override DateTimeOffset UtcNowWithoutTime() => customDate;
    public override DateTimeOffset UtcNow() => customDate;
}