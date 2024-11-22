import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

export const ShowHrPanel = (params: {
    leaveRequests: LeaveRequestDto[] | undefined;
    leaveTypes: LeaveTypeDto[] | undefined;
    leaveLimits: LeaveLimitDto[] | undefined;
  }): JSX.Element => {
    return <>Hr panel</>
}