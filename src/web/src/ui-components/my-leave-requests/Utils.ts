import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";

export function filterLeaveRequests(
  leaveRequests: LeaveRequestDto[] | undefined,
  leaveTypeId: string | undefined,
) {
  return leaveRequests?.filter((x) => x.leaveTypeId === leaveTypeId);
}
