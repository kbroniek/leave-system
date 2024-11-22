import { GridValidRowModel } from "@mui/x-data-grid/models";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { UserDto } from "../dtos/UserDto";
import { LimitsCalculator } from "../utils/LimitsCalculator";
import { DurationFormatter } from "../utils/DurationFormatter";
import { Duration } from "luxon";

export class HrTransformer {
  constructor(
    private readonly employees: UserDto[],
    private readonly leaveRequests: LeaveRequestDto[] | undefined,
    private readonly holidays: string[] | undefined,
    private readonly leaveTypes: LeaveTypeDto[] | undefined,
    private readonly leaveLimits: LeaveLimitDto[] | undefined,
  ) {}

  public transform(): GridValidRowModel[] {
    const holidayLeaveType = this.leaveTypes?.find(
      (x) => x.properties?.catalog === "Holiday",
    );
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
          x.assignedTo.id === employee.id &&
          x.leaveTypeId === holidayLeaveType?.id,
      );
      const approvedLeaveRequests =
        HrTransformer.getCountApprovedLeaveRequests(
          leaveRequestsPerEmployee,
        );
      const limitLeft = approvedLeaveRequests ? limits.totalLimit?.minus(approvedLeaveRequests) : undefined
      const row = {
        id: employee.id,
        totalLimit: formattedLimits.totalLimit,
        limit: formattedLimits.limit,
        overdueLimit: formattedLimits.overdueLimit,
        limitLeft: HrTransformer.formatDuration(limitLeft, workingHours),
        leaveTaken: HrTransformer.formatDuration(approvedLeaveRequests, workingHours, "0d")
      };
      result.push(row);
    }
    return result;
  }
  static getCountApprovedLeaveRequests(
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

  private static formatDuration(duration: Duration | undefined, workingHours: string | undefined, defaultValue: string = "-") {
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
    const { limit, overdueLimit, totalLimit } =
      LimitsCalculator.calculateLimits(...limitPerEmployee);
    return {
      totalLimit: totalLimit ? totalLimit : undefined,
      limit: limit ? limit : undefined,
      overdueLimit: overdueLimit ? overdueLimit : undefined,
    };
  }
}
