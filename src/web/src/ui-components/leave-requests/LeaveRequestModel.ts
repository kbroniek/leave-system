import { DateTime } from "luxon";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";

export type LeaveRequest = LeaveRequestDto & {
    dateFrom: DateTime;
    dateTo: DateTime;
  };