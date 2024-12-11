import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveStatusDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import Grid from "@mui/material/Grid2";
import { MyLeaveRequestsInfo } from "./MyLeaveRequestsInfo";
import { MyLeaveRequestsTable } from "./MyLeaveRequestsTable";
import { Box, Divider, Typography } from "@mui/material";

export const ShowMyLeaveRequests = (params: {
  leaveRequests: LeaveRequestDto[] | undefined;
  leaveStatuses: LeaveStatusDto[] | undefined;
  leaveTypes: LeaveTypeDto[] | undefined;
  leaveLimits: LeaveLimitDto[] | undefined;
}): JSX.Element => {
  return (
    <Grid container spacing={2} sx={{ width: "100%" }}>
      <Grid size={12}>
        <Box sx={{ flexGrow: 1 }} margin={2}>
          <Typography variant="h4">
            {
              params.leaveRequests?.find((x) => x.assignedTo.name)?.assignedTo
                .name
            }
          </Typography>
          <Divider />
        </Box>
      </Grid>
      <Grid size={{ xs: 12, sm: 12, md: 6 }}>
        <MyLeaveRequestsInfo
          leaveRequests={params.leaveRequests?.filter(isValid)}
          leaveTypes={params.leaveTypes}
          leaveLimits={params.leaveLimits}
        />
      </Grid>
      <Grid size={{ xs: 12, sm: 12, md: 6 }}>
        <MyLeaveRequestsTable
          leaveRequests={params.leaveRequests}
          leaveTypes={params.leaveTypes}
          leaveStatuses={params.leaveStatuses}
        />
      </Grid>
    </Grid>
  );
};

function isValid(leaveRequest: LeaveRequestDto): boolean {
  return (
    leaveRequest.status === "Pending" || leaveRequest.status === "Accepted"
  );
}
