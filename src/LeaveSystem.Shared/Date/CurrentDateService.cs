namespace LeaveSystem.Shared.Date;

public class CurrentDateService
{
    public virtual DateTimeOffset UtcNowWithoutTime() => DateTimeOffset.UtcNow.GetDayWithoutTime();
    public virtual DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
}