import { useState } from "react";
import { GridRenderCellParams } from "@mui/x-data-grid/models";
import { DateTime } from "luxon";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import List from "@mui/material/List";
import ListItemButton from "@mui/material/ListItemButton";
import { rowHeight } from "./ShowLeaveRequestsTimeline";
import { RenderLeaveRequestModel } from "./RenderLeaveRequestModel";
import Tooltip from "@mui/material/Tooltip";
import { LeaveRequestDetailsDialog } from "../leave-request-details/LeaveRequestDetailsDialog";
import { UserDto } from "../dtos/UserDto";
import { DurationFormatter } from "../utils/DurationFormatter";

export function RenderLeaveRequests(props: Readonly<GridRenderCellParams<
  UserDto,
  RenderLeaveRequestModel>>): JSX.Element {
  const [openDialog, setOpenDialog] = useState(false);
  const handleClickOpen = () => {
    setOpenDialog(true);
  };

  const handleClose = () => {
    setOpenDialog(false);
  };
  const holidaysDateTime = props.value?.holidays.items.map(x => DateTime.fromISO(x)) ?? [];
  const style = {
    '& .MuiListItemButton-root': {
      display: "flex",
      justifyContent: "center",
      paddingTop: 0,
      paddingBottom: 0,
      paddingLeft: 0,
      paddingRight: 0,
      cursor: "pointer"
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
    },
    ...props.value?.statuses.reduce(
      (a, x) => ({
        ...a,
      [`.leave-request-${x.leaveRequestStatus}`]: {
        backgroundImage: `-webkit-linear-gradient(-121.5deg, ${x.color}, ${x.color} 50.5%, transparent 50%, transparent 100%)`
      }}),
      {}
    ),
    ...props.value?.leaveTypes.reduce(
      (a, x) => ({
        ...a,
        [`.leave-type-${x.id}`]: {
          backgroundColor: x.properties?.color ?? "transparent",
          "&:hover": {
            backgroundColor: x.properties?.color ?? "transparent",
          }
        },
      }),
      {}
    )
  }
  return (
    <div>
      <List disablePadding key={`${props.value?.date.toISO()}-leave-request-details`} sx={style}>
        {
          props.value?.leaveRequests.map(x => (
            <Tooltip title={getTooltip( x.leaveTypeId)} key={`${x.id}-leave-request-detail`}>
              <div>
                <ListItemButton onClick={handleClickOpen} disableGutters={true} className={getCssClass(x.status, x.leaveTypeId)}>
                  {props.value?.date.equals(x.dateFrom) ? (<div className="leave-request-border-start"></div>) : ""}
                  {props.value?.date.equals(x.dateTo) ? (<div className="leave-request-border-end"></div>) : ""}
                  {formatPerDay(x, holidaysDateTime)}
                </ListItemButton>
                <LeaveRequestDetailsDialog
                    open={openDialog}
                    onClose={handleClose}
                    leaveRequestId={x.id}
                  />
              </div>
            </Tooltip>
          ))
        }
      </List>
    </div>
  )
  function getTooltip(leaveTypeId: string): string | undefined {
    return props.value?.leaveTypes.find(x => x.id === leaveTypeId)?.name;
  }

  function formatPerDay(leaveRequest: LeaveRequestDto | undefined, holidays: DateTime[]): string {
    if (!leaveRequest) {
      return "";
    }
    try {
      const formatter = new DurationFormatter(holidays, props.value?.leaveTypes ?? []);
      //TODO: Format with current date i.e. 9h should split with 8h and 1h.
      return formatter.formatPerDay(leaveRequest);
    }
    catch (e) {
      //TODO: log invalid date
      console.warn(e);
      return "";
    }
  }
}

function getCssClass(status: string, leaveTypeId: string): string {
  return `leave-request-${status} leave-type-${leaveTypeId}`;
}

