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
        if (firstDateTo is null && secondDateTo is null)
        {
            return firstDateTo == secondDateTo;
        }
        if (firstDateTo is null)
        {
            return DateIsInPeriod(secondDateFrom, secondDateTo, firstDateFrom);
        }
        if (secondDateTo is null)
        {
            return DateIsInPeriod(firstDateFrom, firstDateTo, secondDateFrom);
        }

        return firstDateFrom < secondDateTo && secondDateFrom < firstDateTo;
    }

    public static bool PeriodsOverlap(DateTimeOffset? firstDateFrom, DateTimeOffset? firstDateTo,
        DateTimeOffset? secondDateFrom, DateTimeOffset? secondDateTo)
    {
        if (firstDateFrom > firstDateTo || secondDateFrom > secondDateTo)
            throw new ArgumentOutOfRangeException(nameof(firstDateFrom));
        var firstDateFromIsNull = firstDateFrom is null;
        var firstDateToIsNull = firstDateTo is null;
        var secondDateFromIsNull = secondDateFrom is null;
        var secondDateToIsNull = secondDateTo is null;
        var cantComparePeriods =
            (firstDateFromIsNull && firstDateToIsNull) || (secondDateFromIsNull && secondDateToIsNull)
                                                       || (firstDateFromIsNull && secondDateFromIsNull)
                                                       || (firstDateToIsNull && secondDateToIsNull);
        return !cantComparePeriods
               && (
                   (firstDateToIsNull && DateIsInPeriod(secondDateFrom, secondDateTo, firstDateFrom!.Value))
                   || (secondDateToIsNull && DateIsInPeriod(firstDateFrom, firstDateTo, secondDateFrom!.Value))
                   || (firstDateFromIsNull && DateIsInPeriod(secondDateFrom, secondDateTo, firstDateTo!.Value))
                   || (secondDateFromIsNull && DateIsInPeriod(firstDateFrom, firstDateTo, secondDateTo!.Value))
                   || (firstDateFrom < secondDateTo && secondDateFrom < firstDateTo)
               );
    }

    public static bool DateIsInPeriod(DateTimeOffset? dateFrom, DateTimeOffset? dateTo, DateTimeOffset date) =>
        date >= dateFrom && date >= dateTo;
}
