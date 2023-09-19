namespace LeaveSystem.Periods;

public static class PeriodResolver
{
    public static bool PeriodsOverlap(DateTimeOffset firstDateFrom, DateTimeOffset? firstDateTo,
        DateTimeOffset secondDateFrom, DateTimeOffset? secondDateTo)
    {
        if (firstDateFrom > firstDateTo || secondDateFrom > secondDateTo)
            throw new ArgumentOutOfRangeException();
        if (firstDateTo == null && secondDateTo == null)
            return firstDateTo == secondDateTo;
        if (firstDateTo == null)
            return DateIsInPeriod(secondDateFrom, secondDateTo, firstDateFrom);
        if (secondDateTo == null)
            return DateIsInPeriod(firstDateFrom, firstDateTo, secondDateFrom);
        return firstDateFrom < secondDateTo && secondDateFrom < firstDateTo;
    }
    
    public static bool DateIsInPeriod(DateTimeOffset dateFrom, DateTimeOffset? dateTo, DateTimeOffset date) => 
        date >= dateFrom && date >= dateTo;
}