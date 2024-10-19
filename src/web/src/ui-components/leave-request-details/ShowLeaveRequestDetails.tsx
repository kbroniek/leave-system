import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";

export default function ShowLeaveRequestsTimeline(params: {
    leaveRequest: LeaveRequestDetailsDto,
}): JSX.Element {

    return (<pre>{JSON.stringify(params.leaveRequest)}</pre>)
}