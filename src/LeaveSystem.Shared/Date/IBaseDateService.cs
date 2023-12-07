namespace LeaveSystem.Shared.Date;

public interface IBaseDateService
{
    public DateTimeOffset GetWithoutTime();
}