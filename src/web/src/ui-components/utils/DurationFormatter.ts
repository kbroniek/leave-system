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
    const workingHours = Duration.fromISO(leaveRequest.workingHours);
    if (!workingHours.isValid) {
      throw Error(
        `Invalid working hours for leave request ${leaveRequest.leaveTypeId}. Duration: ${leaveRequest.duration}`
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
    return DurationFormatter.format(durationPerDay);
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

  public static format(duration: Duration | string): string {
    if(!DurationFormatter.isDuration(duration)) {
      const buffer = Duration.fromISO(duration);
      if (!buffer.isValid) {
        console.warn(`Invalid duration: ${duration}`)
        return "";
      }
      duration = Duration.fromObject({hours: buffer.as("hours")});
    }
    const timeResult = [];
    if (duration.days !== 0) {
      timeResult.push(`${duration.days}d`);
    }
    if (duration.hours !== 0) {
      timeResult.push(`${duration.hours}h`);
    }
    if (duration.minutes !== 0) {
      timeResult.push(`${duration.minutes}m`);
    }
    return timeResult.join(" ");
  }
  public static isDuration(duration: Duration | string): duration is Duration {
    return (<Duration>duration).minus !== undefined;
  }
}
