import { DateTime, Duration } from "luxon";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
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

  private countDays(leaveRequest: LeaveRequestDto): number {
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

  public static days(duration: Duration | string, workingHours: Duration | string) : number {
    if(!DurationFormatter.isDuration(duration)) {
      const durationBuffer = Duration.fromISO(duration);
      if (!durationBuffer.isValid) {
        console.warn(`Invalid duration: ${duration}`);
        return 0;
      }
      duration = Duration.fromObject({hours: durationBuffer.as("hours")});
    }
    if(!DurationFormatter.isDuration(workingHours)) {
      const workingHoursBuffer = Duration.fromISO(workingHours);
      if (!workingHoursBuffer.isValid) {
        console.warn(`Invalid workingHours: ${workingHours}`)
        return 0;
      }
      workingHours = Duration.fromObject({hours: workingHoursBuffer.as("hours")});
    }
    const days = Math.round(duration.as("hours") / workingHours.as("hours"));
    return days;
  }

  public static format(duration: Duration | string, workingHours?: Duration | string): string {
    if(!DurationFormatter.isDuration(duration)) {
      const durationBuffer = Duration.fromISO(duration);
      if (!durationBuffer.isValid) {
        console.warn(`Invalid duration: ${duration}`)
        return "";
      }
      duration = Duration.fromObject({hours: durationBuffer.as("hours")});
    }
    const timeResult = [];
    if(workingHours) {
      if(!DurationFormatter.isDuration(workingHours)) {
        const workingHoursBuffer = Duration.fromISO(workingHours);
        if (!workingHoursBuffer.isValid) {
          console.warn(`Invalid workingHours: ${workingHours}`)
          return "";
        }
        workingHours = Duration.fromObject({hours: workingHoursBuffer.as("hours")});
      }
      const days = Math.round(duration.as("hours") / workingHours.as("hours"));
      //TODO: Could be potential issue.
      const daysDuration = Duration.fromObject({ hours: workingHours.as("hours") * days});
      const durationLeft = duration.minus(daysDuration).normalize();
      timeResult.push(`${days}d`);
      if (durationLeft.hours !== 0) {
        timeResult.push(`${durationLeft.hours}h`);
      }
      if (durationLeft.minutes !== 0) {
        timeResult.push(`${durationLeft.minutes}m`);
      }
    }
    else {
      if (duration.days !== 0) {
        timeResult.push(`${duration.days}d`);
      }
      if (duration.hours !== 0) {
        timeResult.push(`${duration.hours}h`);
      }
      if (duration.minutes !== 0) {
        timeResult.push(`${duration.minutes}m`);
      }
    }
    return timeResult.length === 0 ? "0h" : timeResult.join(" ");
  }
  public static isDuration(duration: Duration | string): duration is Duration {
    return (<Duration>duration).minus !== undefined;
  }
}
