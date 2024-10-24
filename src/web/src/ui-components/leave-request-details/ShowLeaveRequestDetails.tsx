import Typography from "@mui/material/Typography";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import Divider from "@mui/material/Divider";
import { DaysCounter } from "../utils/DaysCounter";
import { HolidaysDto } from "../leave-requests/HolidaysDto";
import { DateTime, DateTimeFormatOptions } from "luxon";
import { DurationFormatter } from "../utils/DurationFormatter";
import Grid from '@mui/material/Grid2';
import Box from '@mui/material/Box';

export default function ShowLeaveRequestsTimeline(params: Readonly<{
  leaveRequest: LeaveRequestDetailsDto;
  statusColor: string,
  leaveType: LeaveTypeDto
  holidays: HolidaysDto
}>): JSX.Element {
  const titleStyle = {color: "text.secondary", textAlign: "right"};
  const defaultStyle = { paddingTop: "1px", width: "max-content" };
  const leaveTypeStyle = { ...defaultStyle, borderBottomColor: params.leaveType.properties?.color ?? "transparent" , borderBottomStyle: "solid" };
  const leaveStatusStyle = { ...defaultStyle, borderBottomColor: params.statusColor, borderBottomStyle: "solid" };
  const holidaysDateTime = params.holidays.items.map(x => DateTime.fromISO(x));
  const daysCounter = new DaysCounter(params.leaveType.properties?.includeFreeDays ?? false, holidaysDateTime);
  const dateFrom = DateTime.fromISO(params.leaveRequest.dateFrom);
  const dateTo = DateTime.fromISO(params.leaveRequest.dateTo);
  const createdDate = DateTime.fromISO(params.leaveRequest.createdDate);
  const lastModifiedDate = DateTime.fromISO(params.leaveRequest.lastModifiedDate);
  const formatDate: DateTimeFormatOptions = { weekday: 'short', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' };
  return (
    <Box sx={{ flexGrow: 1 }} margin={2}>
      <Grid container spacing={2}>
        <Grid size={12}>
          <Typography variant="h5">{params.leaveRequest.assignedTo.name}</Typography>
          <Divider />
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Request type:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={leaveTypeStyle}>{params.leaveType.name}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Leave from - to:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.dateFrom} - {params.leaveRequest.dateTo}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Days:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>{daysCounter.days(dateFrom, dateTo)}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Hours:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>{DurationFormatter.format(params.leaveRequest.duration)}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Created date:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>{createdDate.toLocaleString(formatDate)}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Status:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={leaveStatusStyle}>{params.leaveRequest.status}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Last modified by:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.lastModifiedBy.name}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Last modified date:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>{lastModifiedDate.toLocaleString(formatDate)}</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>Remarks:</Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>{params.leaveRequest.remarks.map(x => x.remarks).join(" | ")}</Typography>
        </Grid>
      </Grid>
    </Box>
  );
}
