namespace LeaveSystem.Shared.Date;

public class DateService
{
    public virtual DateTimeOffset UtcNowWithoutTime() => DateTimeOffset.UtcNow.GetDayWithoutTime();
    public virtual DateTimeOffset UtcNow() => DateTimeOffset.UtcNow.GetDayWithoutTime();
}