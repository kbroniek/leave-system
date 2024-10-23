import { DateTime } from "luxon";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

export class DaysCounter {
  constructor(
    private readonly includeFreeDays: boolean,
    private readonly holidays: DateTime[]
  ) {}

  public static create(
    leaveTypeId: string,
    leaveTypes: LeaveTypeDto[],
    holidays: DateTime[]
  ): DaysCounter {
    const leaveType = leaveTypes.find((x) => x.id === leaveTypeId);
    return new DaysCounter(!!leaveType?.properties?.includeFreeDays, holidays);
  }

  public days(dateFrom: DateTime, dateTo: DateTime): number {
    if (this.includeFreeDays) {
      return DaysCounter.countAllDays(dateTo, dateFrom);
    }
    return this.workingDays(dateFrom, dateTo);
  }

  private static countAllDays(dateTo: DateTime<boolean>, dateFrom: DateTime<boolean>): number {
    return dateTo.plus({ day: 1 }).diff(dateFrom, ["days"]).days;
  }

  private workingDays(dateFrom: DateTime<boolean>, dateTo: DateTime<boolean>) {
    let currentDate = dateFrom;
    let numberOfDays = 1;
    // Max one year
    for (
      let i = 0;
      i < 365 && currentDate < dateTo;
      ++i, currentDate = currentDate.plus({ days: 1 })
    ) {
      if (
        !currentDate.isWeekend &&
        !this.holidays.find((x) => x.equals(currentDate))
      ) {
        ++numberOfDays;
      }
    }
    return numberOfDays;
  }
}
