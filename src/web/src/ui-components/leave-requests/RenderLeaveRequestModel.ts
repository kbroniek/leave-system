import { DateTime } from "luxon";
import { LeaveRequest } from "./LeaveRequestModel";
import { LeaveStatusDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

export interface RenderLeaveRequestModel {
    date: DateTime,
    leaveRequests: LeaveRequest[],
    statuses: LeaveStatusDto[],
    leaveTypes: LeaveTypeDto[]
  }