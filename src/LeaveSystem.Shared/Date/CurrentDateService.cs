namespace LeaveSystem.Shared.Date;

public class CurrentDateService : IBaseDateService
{
    public virtual DateTimeOffset GetWithoutTime() => DateTimeOffset.UtcNow.GetDayWithoutTime();
}