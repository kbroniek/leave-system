import { DateTime } from "luxon";
import { LeaveRequest } from "./LeaveRequestModel";
import { LeaveStatusDto } from "./LeaveStatusDto";
import { LeaveTypeDto } from "./LeaveTypesDto";

export interface RenderLeaveRequestModel {
    date: DateTime,
    leaveRequests: LeaveRequest[],
    statuses: LeaveStatusDto[],
    leaveTypes: LeaveTypeDto[]
  }