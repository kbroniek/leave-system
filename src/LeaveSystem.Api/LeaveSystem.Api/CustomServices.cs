using LeaveSystem.Shared.Date;

namespace LeaveSystem.Api.Seed;

public class CustomDateService : DateService
{
    private readonly DateTimeOffset dateTime;

    public CustomDateService(DateTimeOffset dateTime)
    {
        this.dateTime = dateTime;
    }
    public override DateTimeOffset UtcNowWithoutTime() => dateTime;
}