namespace LeaveSystem.Shared.Date;

public class DateService
{
    public virtual DateTimeOffset GetWithoutTime() => DateTimeOffset.Now.GetDayWithoutTime();
}