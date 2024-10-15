import { DateTime } from "luxon";
import { LeaveRequest } from "./LeaveRequestModel";
import { LeaveStatusDto } from "./LeaveStatusDto";

export interface RenderLeaveRequestModel {
    date: DateTime,
    leaveRequests: LeaveRequest[],
    statuses: LeaveStatusDto[]
  }