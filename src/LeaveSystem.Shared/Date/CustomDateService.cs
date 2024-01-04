namespace LeaveSystem.Shared.Date;

public class CustomDateService : CurrentDateService
{
    private readonly DateTimeOffset customDate;

    public CustomDateService(DateTimeOffset customDate)
    {
        this.customDate = customDate;
    }

    public override DateTimeOffset UtcNowWithoutTime() => customDate;
}