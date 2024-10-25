import { Typography } from "@mui/material"
import { LeaveRequestsResponseDto } from "../leave-requests/LeaveRequestsDto"
import { HolidaysDto } from "../dtos/HolidaysDto"
import { LeaveTypeDto } from "../dtos/LeaveTypesDto"
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto"

export const SubmitLeaveRequestForm = (props: {
    leaveRequests: LeaveRequestsResponseDto,
    holidays: HolidaysDto,
    leaveTypes: LeaveTypeDto[],
    leaveLimits: LeaveLimitDto[]
}) => {
    return (
        <Typography variant="h2">Add leave request</Typography>
    )
}