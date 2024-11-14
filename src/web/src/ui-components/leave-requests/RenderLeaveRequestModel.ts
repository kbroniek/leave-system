import { DateTime } from "luxon";
import { LeaveRequest } from "./LeaveRequestModel";
import { LeaveStatusDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { HolidaysDto } from "../dtos/HolidaysDto";

export interface RenderLeaveRequestModel {
    date: DateTime,
    leaveRequests: LeaveRequest[],
    statuses: LeaveStatusDto[],
    leaveTypes: LeaveTypeDto[],
    holidays: HolidaysDto
  }