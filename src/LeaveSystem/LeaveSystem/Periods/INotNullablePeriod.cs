namespace LeaveSystem.Periods;

public interface INotNullablePeriod
{
    DateTimeOffset DateFrom { get; }
    DateTimeOffset DateTo { get; }
}