import * as React from "react";

import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import Grid2 from "@mui/material/Grid2";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import Divider from "@mui/material/Divider";
import { DaysCounter } from "../utils/DaysCounter";
import { HolidaysDto } from "../leave-requests/HolidaysDto";
import { DateTime } from "luxon";
import { DurationFormatter } from "../utils/DurationFormatter";

export default function ShowLeaveRequestsTimeline(params: {
  leaveRequest: LeaveRequestDetailsDto;
  statusColor: string,
  leaveType: LeaveTypeDto
  holidays: HolidaysDto
}): JSX.Element {
    const defaultStyle = { paddingTop: "1px" };
    const leaveTypeStyle = { ...defaultStyle, borderBottomColor: params.leaveType.properties?.color ?? "transparent" , borderBottomStyle: "solid" };
    const leaveStatusStyle = { ...defaultStyle, borderBottomColor: params.statusColor, borderBottomStyle: "solid" };
    const holidaysDateTime = params.holidays.items.map(x => DateTime.fromISO(x));
    const daysCounter = new DaysCounter(params.leaveType.properties?.includeFreeDays ?? false, holidaysDateTime);
    const dateFrom = DateTime.fromISO(params.leaveRequest.dateFrom);
    const dateTo = DateTime.fromISO(params.leaveRequest.dateTo);
  return (
    <Stack spacing={2} margin={2}>
      <Typography variant="h5">{params.leaveRequest.assignedTo.name}</Typography>
      <Divider />
      <div>
        <Grid2 container>
            <React.Fragment key={params.leaveRequest.leaveTypeId}>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
              <Typography variant="body1" sx={{ color: "text.secondary" }}>
                Request type:
              </Typography>
              <Typography variant="body2" sx={leaveTypeStyle}>{params.leaveType.name}</Typography>
            </Stack>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Leave from - to:
                </Typography>
                <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.dateFrom} - {params.leaveRequest.dateTo}</Typography>
              </Stack>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Days:
                </Typography>
                <Typography variant="body2" sx={defaultStyle}>{daysCounter.days(dateFrom, dateTo)}</Typography>
              </Stack>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Hours:
                </Typography>
                <Typography variant="body2" sx={defaultStyle}>{DurationFormatter.format(params.leaveRequest.duration)}</Typography>
              </Stack>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Created date:
                </Typography>
                <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.createdDate}</Typography>
              </Stack>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Status:
                </Typography>
                <Typography variant="body2" sx={leaveStatusStyle}>{params.leaveRequest.status}</Typography>
              </Stack>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Last modified:
                </Typography>
                <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.lastModifiedBy.name}</Typography>
              </Stack>
              <Stack
                direction="row"
                spacing={1}
                useFlexGap
                sx={{ width: "100%", mb: 1 }}
              >
                <Typography variant="body1" sx={{ color: "text.secondary" }}>
                    Remarks:
                </Typography>
                <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.remarks.map(x => x.remarks).join(" | ")}</Typography>
              </Stack>
            </React.Fragment>
        </Grid2>
      </div>
    </Stack>
  );
}
