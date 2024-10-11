import { GridRenderCellParams } from "@mui/x-data-grid/models";
import { LeaveRequest } from "./LeaveRequestModel";
import { DateTime, Duration } from "luxon";
import { LeaveRequestDto } from "./LeaveRequestsDto";
import List from "@mui/material/List";
import { styled } from "@mui/material/styles";
import ListItemButton from "@mui/material/ListItemButton";

export function RenderLeaveRequests(props: GridRenderCellParams<{id: string}, { date: DateTime, leaveRequests: LeaveRequest[] }>): JSX.Element {
  const LeaveList = styled(List)<{ component?: React.ElementType }>({
    '& .MuiListItemButton-root': {
      display: "flex",
      justifyContent: "center",
      paddingTop: 0,
      paddingBottom: 0
    },
  });
  return (
    <LeaveList disablePadding key={`${props.value?.date.toISO()}-leave-requests`}>
      {
        props.value?.leaveRequests.map(x => (
            <ListItemButton component="a" href="#todo-leave-request-id">
              {mapDuration(x)}
            </ListItemButton>
        ))
      }
    </LeaveList>
  )
  // return mapDuration(props.value?.leaveRequests.find(() => true));
}
function mapDuration(LeaveRequest?: LeaveRequestDto): string {
  if (!LeaveRequest) {
    return "";
  }
  const duration = Duration.fromISO(LeaveRequest.duration);
  if (!duration.isValid) {
    //TODO: log invalid date
    return "";
  }
  const dateFrom = DateTime.fromISO(LeaveRequest.dateFrom);
  const dateTo = DateTime.fromISO(LeaveRequest.dateTo);
  const diff = dateTo.plus({ day: 1 }).diff(dateFrom, ["days"]);
  // https://github.com/moment/luxon/issues/422
  const durationPerDay = Duration.fromObject({
    days: 0,
    hours: 0,
    seconds: 0,
    milliseconds: duration.as("milliseconds") / diff.days,
  }).normalize();
  const timeResult = [];
  if (durationPerDay.days !== 0) {
    timeResult.push(`${durationPerDay.days}d`);
  }
  if (durationPerDay.hours !== 0) {
    timeResult.push(`${durationPerDay.hours}h`);
  }
  if (durationPerDay.minutes !== 0) {
    timeResult.push(`${durationPerDay.minutes}m`);
  }
  return timeResult.join(" ");
}
