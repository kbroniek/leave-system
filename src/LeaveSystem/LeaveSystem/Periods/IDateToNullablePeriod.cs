namespace LeaveSystem.Periods;

public interface IDateToNullablePeriod
{
    DateTimeOffset DateFrom { get; }
    DateTimeOffset? DateTo { get; }
}