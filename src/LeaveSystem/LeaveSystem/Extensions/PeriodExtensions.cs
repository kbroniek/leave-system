using LeaveSystem.Periods;
using LeaveSystem.Shared;

namespace LeaveSystem.Extensions;

public static class PeriodExtensions
{
    public static bool PeriodsOverlap(this IDateToNullablePeriod source, IDateToNullablePeriod value) =>
        Overlap(source.DateFrom.GetDayWithoutTime(),
            source.DateTo?.GetDayWithoutTime() ?? DateTimeOffset.MaxValue,
            value.DateFrom.GetDayWithoutTime(),
            value.DateTo?.GetDayWithoutTime() ?? DateTimeOffset.MaxValue);
    
    public static bool PeriodsOverlap(this INotNullablePeriod source, IDateToNullablePeriod value) =>
        Overlap(source.DateFrom.GetDayWithoutTime(),
            source.DateTo,
            value.DateFrom.GetDayWithoutTime(),
            value.DateTo?.GetDayWithoutTime() ?? DateTimeOffset.MaxValue);

    private static bool Overlap(DateTimeOffset sourceDateFrom, DateTimeOffset? sourceDateTo,
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
        return sourceDateFrom < valueDateTo && valueDateFrom < sourceDateTo;
    }

    private static bool DateIsInPeriod(DateTimeOffset dateFrom, DateTimeOffset? dateTo, DateTimeOffset date) => 
        date >= dateFrom && date >= dateTo;
}