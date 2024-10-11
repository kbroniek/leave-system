import { DateTime } from "luxon";
import { LeaveRequestDto } from "./LeaveRequestsDto";

export type LeaveRequest = LeaveRequestDto & {
    dateFrom: DateTime;
    dateTo: DateTime;
  };