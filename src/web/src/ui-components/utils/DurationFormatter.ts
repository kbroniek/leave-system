import { DateTime, Duration } from "luxon";
import { LeaveRequestDto } from "../leave-requests/LeaveRequestsDto";
import { DaysCounter } from "./DaysCounter";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

export class DurationFormatter {
  constructor(
    private readonly holidays: DateTime[],
    private readonly leaveTypes: LeaveTypeDto[]
  ) {}

  public formatPerDay(leaveRequest: LeaveRequestDto) {
    const duration = Duration.fromISO(leaveRequest.duration);
    if (!duration.isValid) {
      throw Error(
        `Invalid duration for leave request ${leaveRequest.leaveTypeId}. Duration: ${leaveRequest.duration}`
      );
    }
    const diffDays = this.countDays(leaveRequest);
    // https://github.com/moment/luxon/issues/422
    const durationPerDay = Duration.fromObject({
      days: 0,
      hours: 0,
      seconds: 0,
      milliseconds: duration.as("milliseconds") / diffDays,
    }).normalize();
    return this.createResult(durationPerDay);
  }

  private countDays(leaveRequest: LeaveRequestDto) {
    const dateFrom = DateTime.fromISO(leaveRequest.dateFrom);
    const dateTo = DateTime.fromISO(leaveRequest.dateTo);
    const daysCounter = DaysCounter.create(
      leaveRequest.leaveTypeId,
      this.leaveTypes,
      this.holidays
    );
    const diffDays = daysCounter.days(dateFrom, dateTo);
    return diffDays;
  }

  private createResult(durationPerDay: Duration) {
    const timeResult = [];
    if (durationPerDay.days !== 0) {
      timeResult.push(`${durationPerDay.days}d`);
    }
    if (durationPerDay.hours !== 0) {
      timeResult.push(`${durationPerDay.hours}h`);
    }
    if (durationPerDay.minutes !== 0) {
      timeResult.push(`${durationPerDay.minutes}m`);
    }
    return timeResult.join(" ");
  }
}
