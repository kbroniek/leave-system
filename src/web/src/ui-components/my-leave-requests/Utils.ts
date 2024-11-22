import { Duration } from "luxon";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";

export const isString = (item: string | null): item is string => {
  return !!item;
};

export function calculateTotalDuration(durations: string[]): Duration {
  return durations
    .map((x) => Duration.fromISO(x))
    .reduce(
      (accumulator, current) => accumulator.plus(current),
      Duration.fromMillis(0),
    );
}
export function calculateLimits(limits: LeaveLimitDto[] | undefined) {
  if (!limits) {
    return { limit: undefined, overdueLimit: undefined, totalLimit: undefined };
  }
  const limit = calculateTotalDuration(
    limits.map((x) => x.limit).filter(isString),
  );
  const overdueLimit = calculateTotalDuration(
    limits.map((x) => x.overdueLimit).filter(isString),
  );
  const totalLimit = overdueLimit ? limit?.plus(overdueLimit) : undefined;
  return { limit, overdueLimit, totalLimit };
}

export function filterLeaveRequests(
  leaveRequests: LeaveRequestDto[] | undefined,
  leaveTypeId: string | undefined,
) {
  return leaveRequests?.filter((x) => x.leaveTypeId === leaveTypeId);
}
