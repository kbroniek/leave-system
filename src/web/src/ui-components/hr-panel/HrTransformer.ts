import { GridValidRowModel } from "@mui/x-data-grid/models";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { LimitsCalculator } from "../utils/LimitsCalculator";
import { DurationFormatter } from "../utils/DurationFormatter";
import { DateTime, Duration } from "luxon";
import { DaysCounter } from "../utils/DaysCounter";

export class HrTransformer {
  private readonly holidaysConverted: DateTime[];
  constructor(
    private readonly employees: EmployeeDto[],
    private readonly leaveRequests: LeaveRequestDto[] | undefined,
    private readonly holidays: string[] | undefined,
    private readonly leaveTypes: LeaveTypeDto[] | undefined,
    private readonly leaveLimits: LeaveLimitDto[] | undefined,
    private readonly selectedXDaysNumber: number,
  ) {
    this.holidaysConverted = this.holidays?.map((x) => DateTime.fromISO(x)) ?? [];
  }

  public transform(): GridValidRowModel[] {
    const holidayLeaveType = this.leaveTypes?.find(
      (x) => x.properties?.catalog === "Holiday",
    );
    const leaveTypesLeft = this.leaveTypes?.filter(
      (x) => x.properties?.catalog !== "Holiday",
    ) ?? [];
    const result: GridValidRowModel[] = [];
    for (const employee of this.employees) {
      const limitPerEmployee = this.leaveLimits?.filter(
        (x) =>
          x.assignedToUserId === employee.id &&
          x.leaveTypeId === holidayLeaveType?.id,
      );
      const limits = HrTransformer.getLimits(limitPerEmployee);
      const workingHours = limitPerEmployee?.find(() => true)?.workingHours;
      const formattedLimits = HrTransformer.formatLimits(
        limits.totalLimit,
        limits.limit,
        limits.overdueLimit,
        workingHours,
      );
      const leaveRequestsPerEmployee = this.leaveRequests?.filter(
        (x) =>
          x.status === "Accepted" &&
          x.assignedTo.id === employee.id &&
          x.leaveTypeId === holidayLeaveType?.id,
      );
      const approvedLeaveRequests = HrTransformer.getCountApprovedLeaveRequests(
        leaveRequestsPerEmployee,
      );
      const limitLeft = approvedLeaveRequests
        ? limits.totalLimit?.minus(approvedLeaveRequests)
        : undefined;
      const selectedXDays = this.calculateSelectedXDays(
        leaveRequestsPerEmployee,
      );
      const row = {
        id: employee.id,
        totalLimit: formattedLimits.totalLimit,
        limit: formattedLimits.limit,
        overdueLimit: formattedLimits.overdueLimit,
        limitLeft: HrTransformer.formatDuration(limitLeft, workingHours),
        leaveTaken: HrTransformer.formatDuration(
          approvedLeaveRequests,
          workingHours,
          "0d",
        ),
        selectedXDays: selectedXDays,
        ...leaveTypesLeft.reduce(
          (a, v) => ({
            ...a,
            [v.id]: this.calculateDaysTaken(v, employee.id),
          }),
          {}
        ),
      };
      result.push(row);
    }
    return result;
  }
  private calculateSelectedXDays(
    leaveRequests: LeaveRequestDto[] | undefined,
  ): boolean {
    if (!leaveRequests) {
      return false;
    }
    for (const leaveRequest of leaveRequests) {
      const dateFrom = DateTime.fromISO(leaveRequest.dateFrom);
      const dateTo = DateTime.fromISO(leaveRequest.dateTo);
      const numberOdDays = DaysCounter.countAllDays(dateFrom, dateTo);
      if (numberOdDays >= this.selectedXDaysNumber) {
        return true;
      }
      const freeDaysBefore = this.calculateFreeDays(dateFrom, -1);
      if (numberOdDays + freeDaysBefore >= this.selectedXDaysNumber) {
        return true;
      }
      const freeDaysAfter = this.calculateFreeDays(dateTo, 1);
      if (
        numberOdDays + freeDaysBefore + freeDaysAfter >=
        this.selectedXDaysNumber
      ) {
        return true;
      }
    }
    return false;
  }
  private calculateFreeDays(date: DateTime, indicator: number): number {
    let counter = 0;
    for (
      let currentDay = date.plus({ day: indicator });
      (currentDay.isWeekend || this.holidaysConverted.find(x => x.equals(currentDay))) && counter <= this.selectedXDaysNumber;
      currentDay = currentDay.plus({ day: indicator })
    ) {
      ++counter;
    }
    return counter;
  }
  private static getCountApprovedLeaveRequests(
    leaveRequests: LeaveRequestDto[] | undefined,
  ) {
    if (!leaveRequests) {
      return undefined;
    }
    const duration = LimitsCalculator.calculateTotalDuration(
      ...leaveRequests
        .filter((x) => x.status === "Accepted")
        .map((x) => x.duration),
    );
    return duration;
  }

  private static formatDuration(
    duration: Duration | undefined,
    workingHours: string | undefined,
    defaultValue: string = "-",
  ) {
    return duration
      ? DurationFormatter.format(duration, workingHours)
      : defaultValue;
  }
  private static formatLimits(
    totalLimit: Duration | undefined,
    limit: Duration | undefined,
    overdueLimit: Duration | undefined,
    workingHours: string | undefined,
  ) {
    return {
      totalLimit: HrTransformer.formatDuration(totalLimit, workingHours),
      limit: HrTransformer.formatDuration(limit, workingHours),
      overdueLimit: HrTransformer.formatDuration(overdueLimit, workingHours),
    };
  }
  private static getLimits(limitPerEmployee: LeaveLimitDto[] | undefined) {
    if (!limitPerEmployee) {
      return {
        totalLimit: undefined,
        limit: undefined,
        overdueLimit: undefined,
      };
    }
    return LimitsCalculator.calculateLimits(...limitPerEmployee);
  }
  private calculateDaysTaken(leaveType: LeaveTypeDto, employeeId: string): string {
    if(!this.leaveLimits && !this.leaveRequests) {
      return "";
    }
    const leaveRequests = this.leaveRequests?.filter(x => x.assignedTo.id === employeeId && x.leaveTypeId === leaveType.id) ?? [];
    const leaveRequestsUsed = LimitsCalculator.calculateTotalDuration(
      ...leaveRequests
        .filter((x) => x.status === "Accepted")
        .map((x) => x.duration),
    );
    const limits = this.leaveLimits?.filter(x => x.assignedToUserId === employeeId && x.leaveTypeId === leaveType.id);
    if(limits && limits.length > 0) {
      const calculatedLimits = LimitsCalculator.calculateLimits(...limits);
      const workingHours = limits.find(() => true)?.workingHours;
      return `${DurationFormatter.format(leaveRequestsUsed, workingHours)} / ${DurationFormatter.format(calculatedLimits.totalLimit!, workingHours)}`
    }

    return DurationFormatter.format(leaveRequestsUsed, leaveRequests.find(() => true)?.workingHours)
  }
}
