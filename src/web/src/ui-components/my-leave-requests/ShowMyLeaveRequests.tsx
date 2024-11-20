import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveStatusDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import Grid from "@mui/material/Grid2";
import { MyLeaveRequestsInfo } from "./MyLeaveRequestsInfo";
import { MyLeaveRequestsTable } from "./MyLeaveRequestsTable";

export const ShowMyLeaveRequests = (params: {
  leaveRequests: LeaveRequestDto[] | undefined;
  leaveStatuses: LeaveStatusDto[] | undefined;
  leaveTypes: LeaveTypeDto[] | undefined;
  leaveLimits: LeaveLimitDto[] | undefined;
}) => {
  return <Grid container spacing={2} sx={{width: "100%"}}>
<Grid size={{ sm: 12, md: 6 }}><MyLeaveRequestsInfo /></Grid>
<Grid size={{ sm: 12, md: 6 }}><MyLeaveRequestsTable /></Grid>
  </Grid>;
};
