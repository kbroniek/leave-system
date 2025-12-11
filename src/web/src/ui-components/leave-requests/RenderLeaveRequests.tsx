import { useState } from "react";
import { DateTime } from "luxon";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { rowHeight } from "./ShowLeaveRequestsTimeline";
import { RenderLeaveRequestModel } from "./RenderLeaveRequestModel";
import Tooltip from "@mui/material/Tooltip";
import { LeaveRequestDetailsDialog } from "../leave-request-details/LeaveRequestDetailsDialog";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { DurationFormatter } from "../utils/DurationFormatter";
import Button from "@mui/material/Button";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import Box from "@mui/material/Box";
import AddIcon from "@mui/icons-material/Add";
import { useTranslation } from "react-i18next";

export function RenderLeaveRequests(
  props: Readonly<{
    value: RenderLeaveRequestModel;
    row: EmployeeDto;
    onAddLeaveRequest?: (date: DateTime, employee: EmployeeDto) => void;
  }>,
): JSX.Element {
  const { t } = useTranslation();
  const [leaveRequestId, setLeaveRequestId] = useState<string | undefined>();
  const [menuAnchorEl, setMenuAnchorEl] = useState<null | HTMLElement>(null);

  if (!props.value) {
    return <></>;
  }

  const handleAddLeaveRequest = () => {
    if (props.onAddLeaveRequest && props.value && props.row) {
      props.onAddLeaveRequest(props.value.date, props.row);
    }
  };
  const handleClickOpen = (id: string) => {
    setLeaveRequestId(id);
  };

  const handleClose = () => {
    setLeaveRequestId(undefined);
  };
  const open = Boolean(menuAnchorEl);
  const handleMenuClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setMenuAnchorEl(event.currentTarget);
  };
  const handleMenuClose = () => {
    setMenuAnchorEl(null);
  };
  const handleDialogOpenMenuClose = (id: string) => {
    handleClickOpen(id);
    handleMenuClose();
  };
  const holidaysDateTime =
    props.value?.holidays.items.map((x) => DateTime.fromISO(x)) ?? [];
  const style = {
    "& .MuiListItemButton-root": {
      display: "flex",
      justifyContent: "center",
      paddingTop: 0,
      paddingBottom: 0,
      paddingLeft: 0,
      paddingRight: 0,
      cursor: "pointer",
    },
    ".leave-request-border-start": {
      top: 0,
      left: -1,
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
    ".leave-request-border-menu": {
      height: "85%",
      top: "1px",
    },
    ...props.value?.statuses.reduce(
      (a, x) => ({
        ...a,
        [`.leave-request-${x.leaveRequestStatus}`]: {
          backgroundImage: `-webkit-linear-gradient(-121.5deg, ${x.color}, ${x.color} 50.5%, transparent 50%, transparent 100%)`,
        },
      }),
      {},
    ),
    ...props.value?.leaveTypes.reduce(
      (a, x) => ({
        ...a,
        [`.leave-type-${x.id}`]: {
          backgroundColor: x.properties?.color ?? "transparent",
          "&:hover": {
            backgroundColor: x.properties?.color ?? "transparent",
          },
        },
      }),
      {},
    ),
  };
  return (
    <>
      {props.value.leaveRequests.length === 0 ? (
        // Empty cell - show add button
        <Box
          sx={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            height: "100%",
            width: "100%",
          }}
        >
          <Tooltip title={t("Add leave request")}>
            <Button
              onClick={handleAddLeaveRequest}
              sx={{
                padding: 0,
                minWidth: "50px",
                height: "29px",
                color: "gray",
                "&:hover": {
                  backgroundColor: "rgba(0, 0, 0, 0.04)",
                },
              }}
            >
              <Box
                sx={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  width: "100%",
                  height: "100%",
                  "& .add-icon-hover": {
                    opacity: 0,
                    transition: "opacity 0.2s",
                  },
                  "&:hover .add-icon-hover": {
                    opacity: 1,
                  },
                }}
              >
                <AddIcon fontSize="small" className="add-icon-hover" />
              </Box>
            </Button>
          </Tooltip>
        </Box>
      ) : props.value.leaveRequests.length > 1 ? (
        <>
          <Button
            id="basic-button"
            aria-controls={open ? "basic-menu" : undefined}
            aria-haspopup="true"
            aria-expanded={open ? "true" : undefined}
            onClick={handleMenuClick}
          >
            <ExpandMoreIcon />
          </Button>
          <Menu
            id="basic-menu"
            anchorEl={menuAnchorEl}
            open={open}
            onClose={handleMenuClose}
            MenuListProps={{
              "aria-labelledby": "basic-button",
            }}
            sx={style}
          >
            {props.value?.leaveRequests.map((x) => (
              <MenuItem
                key={`${x.id}-render-leave-request-detail`}
                onClick={() => handleDialogOpenMenuClose(x.id)}
                className={getCssClass(x.status, x.leaveTypeId)}
              >
                {props.value?.date.equals(x.dateFrom) ? (
                  <div className="leave-request-border-start leave-request-border-menu"></div>
                ) : (
                  ""
                )}
                {props.value?.date.equals(x.dateTo) ? (
                  <div className="leave-request-border-end leave-request-border-menu"></div>
                ) : (
                  ""
                )}
                {formatPerDay(x, holidaysDateTime)}
              </MenuItem>
            ))}
          </Menu>
        </>
      ) : (
        <Box
          key={`${props.value?.date.toISO()}-leave-request-details`}
          sx={style}
        >
          {props.value?.leaveRequests.map((x) => (
            <Tooltip
              title={getTooltip(x.leaveTypeId)}
              key={`${x.id}-leave-request-detail`}
            >
              <Button
                variant="text"
                onClick={() => handleClickOpen(x.id)}
                className={getCssClass(x.status, x.leaveTypeId)}
                sx={{
                  padding: 0,
                  minWidth: "50px",
                  height: "29px",
                  color: "black",
                }}
              >
                {props.value?.date.equals(x.dateFrom) ? (
                  <div className="leave-request-border-start"></div>
                ) : (
                  ""
                )}
                {props.value?.date.equals(x.dateTo) ? (
                  <div className="leave-request-border-end"></div>
                ) : (
                  ""
                )}
                {formatPerDay(x, holidaysDateTime)}
              </Button>
            </Tooltip>
          ))}
        </Box>
      )}
      <LeaveRequestDetailsDialog
        open={!!leaveRequestId}
        onClose={handleClose}
        leaveRequestId={leaveRequestId}
      />
    </>
  );

  function getTooltip(leaveTypeId: string): string | undefined {
    return props.value?.leaveTypes.find((x) => x.id === leaveTypeId)?.name;
  }

  function formatPerDay(
    leaveRequest: LeaveRequestDto | undefined,
    holidays: DateTime[],
  ): string {
    if (!leaveRequest) {
      return "";
    }
    try {
      const formatter = new DurationFormatter(
        holidays,
        props.value?.leaveTypes ?? [],
      );
      //TODO: Format with current date i.e. 9h should split with 8h and 1h.
      return formatter.formatPerDay(leaveRequest);
    } catch (e) {
      //TODO: log invalid date
      console.warn(e);
      return "";
    }
  }
}

function getCssClass(status: string, leaveTypeId: string): string {
  return `leave-request-${status} leave-type-${leaveTypeId}`;
}
