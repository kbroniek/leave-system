namespace LeaveSystem.Functions.Holidays;
using System;
using System.Collections.Generic;
using LeaveSystem.Shared;

public class HolidaysService
{
    internal virtual IEnumerable<DateOnly> GetHolidays(DateOnly dateFrom, DateOnly dateTo)
    {
        var dateToPlusOne = dateTo.AddDays(1);
        var currentDate = dateFrom;
        do
        {
            if (DateOnlyCalculator.GetDayKind(currentDate) == DateOnlyCalculator.DayKind.HOLIDAY)
            {
                yield return currentDate;
            }
            currentDate = currentDate.AddDays(1);
        } while (currentDate < dateToPlusOne);
    }
}
