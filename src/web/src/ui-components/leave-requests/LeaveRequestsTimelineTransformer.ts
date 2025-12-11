import { DateTime } from "luxon";
import {
  LeaveRequestDto,
  LeaveRequestsResponseDto,
} from "../dtos/LeaveRequestsDto";
import { LeaveRequest } from "./LeaveRequestModel";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

export class LeaveRequestsTimelineTransformer {
  constructor(
    private readonly employees: EmployeeDto[],
    private readonly leaveRequestsResponse: LeaveRequestsResponseDto,
    private readonly holidays: HolidaysDto,
    private readonly leaveTypes: LeaveTypeDto[],
  ) {}
  public transformToTable(): UserLeaveRequestTableCollection {
    const dateFrom = DateTime.fromISO(
      this.leaveRequestsResponse.search.dateFrom,
    );
    const dateTo = DateTime.fromISO(this.leaveRequestsResponse.search.dateTo);
    // Ensure startDay is before endDay
    if (dateFrom > dateTo) {
      throw new Error("endDay must be after startDay");
    }

    const leaveRequests = this.buildDateTime(this.leaveRequestsResponse.items);
    return {
      items: this.employees.map((x) => ({
        employee: {
          id: x.id,
          name: x.name,
        },
        table: this.transformLeaveRequest(
          leaveRequests.filter((lr) => lr.assignedTo.id === x.id),
          dateFrom,
          dateTo,
        ),
      })),
      header: this.transformHeader(dateFrom, dateTo),
    };
  }
  private buildDateTime(leaveRequests: LeaveRequestDto[]): LeaveRequest[] {
    return leaveRequests.map(
      (x) =>
        ({
          ...x,
          dateFrom: DateTime.fromISO(x.dateFrom),
          dateTo: DateTime.fromISO(x.dateTo),
        }) as LeaveRequest,
    );
  }

  private transformLeaveRequest(
    leaveRequests: LeaveRequest[],
    dateFrom: DateTime,
    dateTo: DateTime,
  ): LeaveRequestTable[] {
    const dateSequence = this.createDatesSequence(dateFrom, dateTo);
    const holidaysDateTime = this.holidays.items.map((x) =>
      DateTime.fromISO(x),
    );

    return dateSequence.map((currentDate) => ({
      date: currentDate,
      leaveRequests: leaveRequests.filter((lr) =>
        this.isLeaveRequestValid(lr, currentDate, holidaysDateTime),
      ),
    }));
  }

  private isLeaveRequestValid(
    leaveRequest: LeaveRequest,
    currentDate: DateTime,
    holidays: DateTime[],
  ): boolean {
    const validFromTo =
      leaveRequest.dateFrom <= currentDate &&
      leaveRequest.dateTo >= currentDate;
    if (!validFromTo) {
      return false;
    }
    const leaveType = this.leaveTypes.find(
      (x) => x.id === leaveRequest.leaveTypeId,
    );
    if (!leaveType?.properties?.includeFreeDays) {
      return (
        !currentDate.isWeekend && !holidays.find((x) => x.equals(currentDate))
      );
    }
    return validFromTo;
  }

  private createDatesSequence(
    dateFrom: DateTime,
    dateTo: DateTime,
  ): DateTime[] {
    let currentDate = dateFrom;
    const sequence: DateTime[] = [];
    // Max one year
    for (
      let i = 0;
      i < 365 && currentDate <= dateTo;
      ++i, currentDate = currentDate.plus({ days: 1 })
    ) {
      sequence.push(currentDate);
    }
    return sequence;
  }

  private transformHeader(dateFrom: DateTime, dateTo: DateTime): HeaderTable[] {
    const headerTable: HeaderTable[] = [];
    const dateToStart = DateTime.fromObject({
      year: dateTo.year,
      month: dateTo.month,
      day: 1,
    });

    // First month - from dateFrom to end of month or dateTo, whichever comes first
    const firstMonthEnd =
      dateFrom.month === dateTo.month && dateFrom.year === dateTo.year
        ? dateTo
        : DateTime.fromObject({
            year: dateFrom.year,
            month: dateFrom.month,
            day: this.getDaysInMonth(dateFrom.year, dateFrom.month),
          });

    const actualFirstMonthEnd = firstMonthEnd > dateTo ? dateTo : firstMonthEnd;

    headerTable.push({
      date: dateFrom,
      days: this.createDatesSequence(dateFrom, actualFirstMonthEnd),
    });

    // Middle months - full months between first and last
    for (
      let currentDate = DateTime.fromObject({
        year: dateFrom.year,
        month: dateFrom.month,
        day: 1,
      }).plus({ month: 1 });
      currentDate < dateToStart;
      currentDate = currentDate.plus({ month: 1 })
    ) {
      const daysLeft =
        this.getDaysInMonth(currentDate.year, currentDate.month) - 1;
      headerTable.push({
        date: currentDate,
        days: this.createDatesSequence(
          currentDate,
          currentDate.plus({ days: daysLeft }),
        ),
      });
    }

    // Last month - only if different from first month
    if (dateFrom.month !== dateTo.month || dateFrom.year !== dateTo.year) {
      headerTable.push({
        date: dateTo,
        days: this.createDatesSequence(dateToStart, dateTo),
      });
    }
    return headerTable;
  }

  private getDaysInMonth(year: number, month: number): number {
    return DateTime.fromObject({ year, month }).daysInMonth!;
  }
}

export interface UserLeaveRequestTableCollection {
  items: UserLeaveRequestTable[];
  header: HeaderTable[];
}
export interface UserLeaveRequestTable {
  employee: {
    name?: string;
    id: string;
  };
  table: LeaveRequestTable[];
}
export interface LeaveRequestTable {
  date: DateTime;
  leaveRequests: LeaveRequest[];
}

export interface HeaderTable {
  days: DateTime[];
  date: DateTime;
}
