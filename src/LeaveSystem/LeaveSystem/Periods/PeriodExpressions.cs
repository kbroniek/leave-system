using System.Linq.Expressions;

namespace LeaveSystem.Periods;

public class PeriodExpressions
{
    public static Expression<Func<TDateToNullablePeriod, bool>> GetPeriodOverlapExp<TNotNullablePeriod,
        TDateToNullablePeriod>(TNotNullablePeriod period)
        where TNotNullablePeriod : INotNullablePeriod where TDateToNullablePeriod : IDateToNullablePeriod =>
        x =>
            (x.DateTo.HasValue && period.DateFrom <= x.DateTo && x.DateFrom <= period.DateTo) ||
            !x.DateTo.HasValue && x.DateFrom >= period.DateFrom && x.DateFrom <= period.DateTo;

    public static Expression<Func<TNotNullablePeriod, bool>> GetPeriodOverlapExp<TNotNullablePeriod,
        TDateToNullablePeriod>(TDateToNullablePeriod period)
        where TNotNullablePeriod : INotNullablePeriod where TDateToNullablePeriod : IDateToNullablePeriod =>
        x =>
            (period.DateTo.HasValue && x.DateFrom <= period.DateTo && period.DateFrom <= x.DateTo) ||
            !period.DateTo.HasValue && period.DateFrom >= x.DateFrom && period.DateFrom <= x.DateTo;

    public static Expression<Func<TNotNullablePeriod, bool>> GetPeriodOverlapExp<TNotNullablePeriod>(
        INotNullablePeriod period)
        where TNotNullablePeriod : INotNullablePeriod =>
        x =>
            period.DateFrom <= x.DateTo && x.DateFrom <= period.DateTo;

    public static Expression<Func<TDateToNullablePeriod, bool>> GetPeriodOverlapExp<TDateToNullablePeriod>(
        IDateToNullablePeriod period)
        where TDateToNullablePeriod : IDateToNullablePeriod =>
        x =>
        // true statements:
        // period = {
        //      DateFrom = 2023-01-01
        //      DateTo = null
        // }
        // x = {
        //      DateFrom = 2023-01-02
        //      DateTo = null
        // }
            (!x.DateTo.HasValue && !period.DateTo.HasValue && x.DateFrom >= period.DateFrom) ||
        // period = {
        //      DateFrom = 2023-01-01
        //      DateTo = 2023-12-31
        // }
        // x = {
        //      DateFrom = 2023-12-30
        //      DateTo = null
        // }
            (!x.DateTo.HasValue && x.DateFrom >= period.DateFrom && x.DateFrom <= period.DateTo) ||
        // period = {
        //      DateFrom = 2023-01-01
        //      DateTo = null
        // }
        // x = {
        //      DateFrom = 2022-12-30
        //      DateTo = 2023-01-02
        // }
            (!period.DateTo.HasValue && x.DateFrom >= period.DateFrom || x.DateTo >= period.DateFrom) ||
        // period = {
        //      DateFrom = 2023-01-01
        //      DateTo = 2023-12-31
        // }
        // x = {
        //      DateFrom = 2023-12-30
        //      DateTo = 2024-01-02
        // }
            (x.DateTo.HasValue && period.DateTo.HasValue &&
            ((x.DateFrom >= period.DateFrom && x.DateFrom <= period.DateTo) ||
        // period = {
        //      DateFrom = 2023-01-01
        //      DateTo = 2023-12-31
        // }
        // x = {
        //      DateFrom = 2022-12-30
        //      DateTo = 2023-01-02
        // }
            (x.DateTo >= period.DateFrom && x.DateTo <= (period.DateTo ?? DateTimeOffset.MinValue)))); // nedded because throws an error
}