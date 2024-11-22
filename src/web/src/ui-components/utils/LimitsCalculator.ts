import { Duration } from "luxon";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";

export class LimitsCalculator {
  public static calculateLimits(... limits: LeaveLimitDto[]) {
    if (limits.length === 0) {
      return {
        limit: undefined,
        overdueLimit: undefined,
        totalLimit: undefined,
      };
    }
    const limit = LimitsCalculator.calculateTotalDuration(
      ...limits.map((x) => x.limit),
    );
    const overdueLimit = LimitsCalculator.calculateTotalDuration(
      ...limits.map((x) => x.overdueLimit),
    );
    const totalLimit = overdueLimit ? limit?.plus(overdueLimit) : limit;
    return { limit, overdueLimit, totalLimit };
  }
  public static calculateTotalDuration(...durations: (string | null | undefined)[]): Duration {
    return durations
      .filter(LimitsCalculator.isString)
      .map((x) => Duration.fromISO(x))
      .reduce(
        (accumulator, current) => accumulator.plus(current),
        Duration.fromMillis(0),
      );
  }

  private static isString = (item: string | null | undefined): item is string => {
    return !!item;
  };
}
