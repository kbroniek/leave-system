using System.Globalization;

namespace LeaveSystem.Shared;

public static class DateCalculator
{
    public const uint MaxCalculatedDays = 366;
    public enum DayKind
    {
        WORKING,
        WEEKEND,
        HOLIDAY
    }
    public static TimeSpan CalculateDuration(DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan workingHours, bool? includeFreeDays)
    {
        var dateToPlusOne = dateTo.AddDays(1).GetDayWithoutTime();
        dateFrom = dateFrom.GetDayWithoutTime();
        if (includeFreeDays == true)
        {
            return (dateToPlusOne - dateFrom).Days * workingHours;
        }
        var currentDate = dateFrom;
        long daysBetween = 0;
        do
        {
            if (GetDayKind(currentDate) == DayKind.WORKING)
            {
                ++daysBetween;
                if (daysBetween > MaxCalculatedDays)
                {
                    throw new ArgumentOutOfRangeException($"Max range reached calculating duration between dates from: {dateFrom.ToString("o", CultureInfo.InvariantCulture)} to: {dateTo.ToString("o", CultureInfo.InvariantCulture)}. The max duration is {MaxCalculatedDays} days.");
                }
            }
            currentDate = currentDate.AddDays(1);
        } while (currentDate < dateToPlusOne);
        return daysBetween * workingHours;
    }

    public static int CalculateDurationIncludeFreeDays(DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        var dateToPlusOne = dateTo.AddDays(1);
        var daysBetween = (dateToPlusOne - dateFrom).Days;
        var currentDate = dateToPlusOne;
        while(GetDayKind(currentDate) != DayKind.WORKING)
        {
            ++daysBetween;
            currentDate = currentDate.AddDays(1);
        }
        currentDate = dateFrom.AddDays(-1);
        while (GetDayKind(currentDate) != DayKind.WORKING)
        {
            ++daysBetween;
            currentDate = currentDate.AddDays(-1);
        }
        return daysBetween;
    }

    public static DateTimeOffset GetNextWorkingDay(DateTimeOffset date)
    {
        while(GetDayKind(date) != DayKind.WORKING)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    public static DayKind GetDayKind(DateTimeOffset date)
    {
        if (isPolishHoliday(date))
        {
            return DayKind.HOLIDAY;
        }

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            return DayKind.WEEKEND;
        }

        return DayKind.WORKING;
    }

    private static bool isPolishHoliday(DateTimeOffset date)
    {
        switch (date.Month)
        {
            case 1: //JANUARY
                {
                    if (date.Day == 1 || date.Day == 6)
                    {
                        return true;
                    }
                    break;
                }
            case 5: //MAY
                {
                    if (date.Day == 1 || date.Day == 3)
                    {
                        return true;
                    }
                    break;
                }
            case 8: //AUGUST
                {
                    if (date.Day == 15)
                    {
                        return true;
                    }
                    break;
                }
            case 11: //NOVEMBER
                {
                    if (date.Day == 1 || date.Day == 11)
                    {
                        return true;
                    }
                    break;
                }
            case 12: //DECEMBER
                {
                    if (date.Day == 25 || date.Day == 26)
                    {
                        return true;
                    }
                    break;
                }
        }

        if (date.Month >= 3 && date.Month <= 6)
        {
            // obliczanie Wielkanocy
            var yearMod19 = date.Year % 19;
            var floorYearDiv100 = (int)Math.Floor((double)(date.Year / 100));
            var yearMod100 = date.Year % 100;
            var floorYearDiv100Div4 = (int)Math.Floor((double)(floorYearDiv100 / 4));
            var floorYearDiv100mod4 = floorYearDiv100 % 4;
            var f = (int)Math.Floor((double)((floorYearDiv100 + 8) / 25));
            var g = (int)Math.Floor((double)((floorYearDiv100 - f + 1) / 3));
            var h = (19 * yearMod19 + floorYearDiv100 - floorYearDiv100Div4 - g + 15) % 30;
            var i = (int)Math.Floor((double)(yearMod100 / 4));
            var yearMod100Mod4 = yearMod100 % 4;
            var l = (32 + 2 * floorYearDiv100mod4 + 2 * i - h - yearMod100Mod4) % 7;
            var m = (int)Math.Floor((double)((yearMod19 + 11 * h + 22 * l) / 451));
            var p = (h + l - 7 * m + 114) % 31;
            var day = p + 1;
            var month = (int)Math.Floor((double)((h + l - 7 * m + 114) / 31));

            DateTimeOffset wielkanoc = new DateTimeOffset(new DateTime(date.Year, month, day), TimeSpan.Zero);

            var dateWithoutTime = date.GetDayWithoutTime();

            return dateWithoutTime == wielkanoc ||
                dateWithoutTime == wielkanoc.AddDays(1) ||
                dateWithoutTime == wielkanoc.AddDays(49) ||
                dateWithoutTime == wielkanoc.AddDays(60);
        }
        return false;
    }
}
