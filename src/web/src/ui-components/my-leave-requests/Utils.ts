import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeCatalog, LeaveTypeDto } from "../dtos/LeaveTypesDto";

export function filterLeaveRequests(
  leaveRequests: LeaveRequestDto[] | undefined,
  leaveTypeIds: (string | undefined)[],
) {
  return leaveRequests?.filter((x) => leaveTypeIds.find(t => t === x.leaveTypeId));
}


export function findLeaveTypes(
  leaveTypes: LeaveTypeDto[] | undefined,
  leaveTypeCatalog: LeaveTypeCatalog,
) {
  return leaveTypes?.find((x) => x.properties?.catalog === leaveTypeCatalog);
}