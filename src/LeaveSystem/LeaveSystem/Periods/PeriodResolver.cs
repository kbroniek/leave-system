namespace LeaveSystem.Periods;
public static class PeriodResolver
{
    public static bool PeriodsOverlap(DateTimeOffset firstDateFrom, DateTimeOffset? firstDateTo,
        DateTimeOffset secondDateFrom, DateTimeOffset? secondDateTo)
    {
        if (firstDateFrom > firstDateTo)
        {
            throw new ArgumentOutOfRangeException(nameof(firstDateFrom), "The DateForm of the first argument can't be grater than the DateTo.");
        }
        if (secondDateFrom > secondDateTo)
        {
            throw new ArgumentOutOfRangeException(nameof(secondDateFrom), "The DateForm of the second argument can't be grater than the DateTo.");
        }
        if (firstDateTo == null && secondDateTo == null)
        {
            return firstDateTo == secondDateTo;
        }
        if (firstDateTo == null)
        {
            return DateIsInPeriod(secondDateFrom, secondDateTo, firstDateFrom);
        }
        if (secondDateTo == null)
        {
            return DateIsInPeriod(firstDateFrom, firstDateTo, secondDateFrom);
        }

        return firstDateFrom < secondDateTo && secondDateFrom < firstDateTo;
    }

    public static bool DateIsInPeriod(DateTimeOffset dateFrom, DateTimeOffset? dateTo, DateTimeOffset date) =>
        date >= dateFrom && date >= dateTo;
}