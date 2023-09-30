namespace LeaveSystem.Shared.Date;

public class CurrentDateService
{
    public virtual DateTimeOffset GetWithoutTime() => DateTimeOffset.Now.GetDayWithoutTime();
}