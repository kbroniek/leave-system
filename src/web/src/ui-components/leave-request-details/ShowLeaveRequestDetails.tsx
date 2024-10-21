import * as React from "react";

import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import Grid2 from "@mui/material/Grid2";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import { LeaveStatusDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import Divider from "@mui/material/Divider";

export default function ShowLeaveRequestsTimeline(params: {
  leaveRequest: LeaveRequestDetailsDto;
  leaveStatuses: LeaveStatusDto[],
  leaveType: LeaveTypeDto
}): JSX.Element {
    const statusColor = params.leaveStatuses.find(x => x.leaveRequestStatus === params.leaveRequest.status)?.color ?? "transparent";
    const defaultStyle = { paddingTop: "1px" };
    const leaveTypeStyle = { ...defaultStyle, borderBottomColor: params.leaveType.properties.color, borderBottomStyle: "solid" };
    const leaveStatusStyle = { ...defaultStyle, borderBottomColor: statusColor, borderBottomStyle: "solid" };
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
                <Typography variant="body2" sx={defaultStyle}>TODO</Typography>
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
                <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.duration}</Typography>
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
