namespace LeaveSystem.Shared.Date;

public class CustomDateService : IBaseDateService
{
    private readonly DateTimeOffset customDate;

    public CustomDateService(DateTimeOffset customDate)
    {
        this.customDate = customDate;
    }

    public virtual DateTimeOffset GetWithoutTime() => customDate;
}