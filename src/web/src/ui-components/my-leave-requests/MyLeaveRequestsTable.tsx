import Typography from "@mui/material/Typography"
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

export const MyLeaveRequestsTable = (params: {
    leaveRequests: LeaveRequestDto[] | undefined;
    leaveTypes: LeaveTypeDto[] | undefined;
  }): JSX.Element => {
    return <Typography variant="h5">Leave requests</Typography>
}