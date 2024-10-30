import { DateTime, Interval } from "luxon";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

export class DaysCounter {
  constructor(
    private readonly holidays: DateTime[],
    private readonly includeFreeDays?: boolean
  ) {}

  public static create(
    leaveTypeId: string,
    leaveTypes: LeaveTypeDto[],
    holidays: DateTime[]
  ): DaysCounter {
    const leaveType = leaveTypes.find((x) => x.id === leaveTypeId);
    return new DaysCounter(holidays, !!leaveType?.properties?.includeFreeDays);
  }

  public days(dateFrom: DateTime, dateTo: DateTime): number {
    if (this.includeFreeDays) {
      return DaysCounter.countAllDays(dateFrom, dateTo);
    }
    return this.workingDays(dateFrom, dateTo);
  }

  public static countAllDays(dateFrom: DateTime<boolean>, dateTo: DateTime<boolean>, ): number {
    return dateTo.plus({ day: 1 }).diff(dateFrom, ["days"]).days;
  }

  public workingDays(dateFrom: DateTime<boolean>, dateTo: DateTime<boolean>) {
    dateFrom = dateFrom.startOf('day');
    dateTo = dateTo.startOf('day').plus({day: 1});

    const interval = Interval.fromDateTimes(dateFrom, dateTo);
    const subIntervals = interval.splitBy({ days: 1 });
    const workingDays = subIntervals.reduce((counter, subInt) => {
      const current = subInt.start;
      return current && !this.isFreeDay(current) ? counter + 1 : counter;
    }, 0);
    return workingDays;
  }
  private isFreeDay(date: DateTime) {
    return date.isWeekend || this.holidays.find((x) => x.equals(date));
  }
}
