using LeaveSystem.Shared.Date;

namespace LeaveSystem.Api.Db;

public class CustomDateService : CurrentDateService
{
    public override DateTimeOffset GetWithoutTime() => DateTimeOffset.Parse("2023-12-01 00:00:00 +01:00");
}