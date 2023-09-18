using LeaveSystem.Periods;
using LeaveSystem.Shared;

namespace LeaveSystem.Extensions;

public static class PeriodExtensions
{
    public static bool PeriodsOverlaps(this IDateToNullablePeriod source, IDateToNullablePeriod value) =>
        Overlaps(source.DateFrom.GetDayWithoutTime(),
            source.DateTo?.GetDayWithoutTime() ?? DateTimeOffset.MaxValue,
            value.DateFrom.GetDayWithoutTime(),
            value.DateTo?.GetDayWithoutTime() ?? DateTimeOffset.MaxValue);

    private static bool Overlaps(DateTimeOffset sourceDateFrom, DateTimeOffset? sourceDateTo,
        DateTimeOffset valueDateFrom, DateTimeOffset? valueDateTo)
    {
        if (sourceDateFrom > sourceDateTo || valueDateFrom > valueDateTo)
            throw new ArgumentOutOfRangeException();
        if (sourceDateTo == null && valueDateTo == null)
            return sourceDateTo == valueDateTo;
        if (sourceDateTo == null)
            return DateIsInPeriod(valueDateFrom, valueDateTo, sourceDateFrom);
        if (valueDateTo == null)
            return DateIsInPeriod(sourceDateFrom, sourceDateTo, valueDateFrom);
        return sourceDateFrom < valueDateTo && valueDateFrom < valueDateTo;
    }

    private static bool DateIsInPeriod(DateTimeOffset dateFrom, DateTimeOffset? dateTo, DateTimeOffset date) => 
        date >= dateFrom && date >= dateTo;
}