import { GridRenderCellParams } from "@mui/x-data-grid/models";
import { DateTime, Duration } from "luxon";
import { LeaveRequestDto } from "./LeaveRequestsDto";
import List from "@mui/material/List";
import { styled } from "@mui/material/styles";
import ListItemButton from "@mui/material/ListItemButton";
import { rowHeight } from "./ShowLeaveRequestsTimeline";
import { RenderLeaveRequestModel } from "./RenderLeaveRequestModel";

export function RenderLeaveRequests(props: GridRenderCellParams<
  { id: string },
  RenderLeaveRequestModel>): JSX.Element {
  const LeaveList = styled(List)<{ component?: React.ElementType }>({
    '& .MuiListItemButton-root': {
      display: "flex",
      justifyContent: "center",
      paddingTop: 0,
      paddingBottom: 0,
      paddingLeft: 0,
      paddingRight: 0
    },
    ".leave-request-border-start": {
      top: 0,
      left: 0,
      position: "absolute",
      height: rowHeight - 4,
      marginLeft: "1px",
      borderLeft: "solid 2px black",
      borderTop: "solid 2px black",
      borderBottom: "solid 2px black",
      zIndex: 400,
      width: "4px",
    },
    ".leave-request-border-end": {
      top: 0,
      right: 0,
      position: "absolute",
      height: rowHeight - 4,
      borderRight: "solid 2px black",
      borderTop: "solid 2px black",
      borderBottom: "solid 2px black",
      zIndex: 400,
      width: "4px",
    }
  });
  return (
    <LeaveList disablePadding key={`${props.value?.date.toISO()}-leave-requests`}>
      {
        props.value?.leaveRequests.map(x => (
          <>
            {props.value?.date.toMillis() === x.dateFrom.toMillis() ? (<div className="leave-request-border-start"></div>) : ""}
            {props.value?.date.toMillis() === x.dateTo.toMillis() ? (<div className="leave-request-border-end"></div>) : ""}
            <ListItemButton component="a" href="#todo-leave-request-id" disableGutters={true}>
              {mapDuration(x)}
            </ListItemButton>
          </>
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
