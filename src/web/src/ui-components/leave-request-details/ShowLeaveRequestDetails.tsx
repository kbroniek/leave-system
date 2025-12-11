import Typography from "@mui/material/Typography";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import Divider from "@mui/material/Divider";
import { DaysCounter } from "../utils/DaysCounter";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { DateTime, DateTimeFormatOptions } from "luxon";
import { DurationFormatter } from "../utils/DurationFormatter";
import Grid from "@mui/material/Grid";
import Box from "@mui/material/Box";
import ButtonGroup from "@mui/material/ButtonGroup";
import LoadingButton from "@mui/lab/LoadingButton";
import CancelIcon from "@mui/icons-material/Cancel";
import ThumbDownIcon from "@mui/icons-material/ThumbDown";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import { useState } from "react";
import TextField from "@mui/material/TextField";
import { Authorized } from "../../components/Authorized";
import LinkIcon from "@mui/icons-material/Link";
import { Button } from "@mui/material";
import { useNotifications } from "@toolpad/core/useNotifications";
import { Trans, useTranslation } from "react-i18next";

export default function ShowLeaveRequestsTimeline(
  props: Readonly<{
    leaveRequest: LeaveRequestDetailsDto;
    statusColor: string;
    leaveType: LeaveTypeDto;
    holidays: HolidaysDto;
    onAccept: (id: string, remarks: string) => Promise<void>;
    onReject: (id: string, remarks: string) => Promise<void>;
    onCancel: (id: string, remarks: string) => Promise<void>;
  }>
): React.ReactElement {
  const titleStyle = { color: "text.secondary", textAlign: "right" };
  const defaultStyle = { paddingTop: "1px", width: "max-content" };
  const leaveTypeStyle = {
    ...defaultStyle,
    borderBottomColor: props.leaveType.properties?.color ?? "transparent",
    borderBottomStyle: "solid",
  };
  const leaveStatusStyle = {
    ...defaultStyle,
    borderBottomColor: props.statusColor,
    borderBottomStyle: "solid",
  };
  const holidaysDateTime = props.holidays.items.map((x) => DateTime.fromISO(x));
  const daysCounter = new DaysCounter(
    holidaysDateTime,
    props.leaveType.properties?.includeFreeDays ?? false
  );
  const dateFrom = DateTime.fromISO(props.leaveRequest.dateFrom);
  const dateTo = DateTime.fromISO(props.leaveRequest.dateTo);
  const createdDate = DateTime.fromISO(props.leaveRequest.createdDate);
  const lastModifiedDate = DateTime.fromISO(
    props.leaveRequest.lastModifiedDate
  );
  const formatDate: DateTimeFormatOptions = {
    weekday: "short",
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  };
  const [actionProgress, setActionProgress] = useState(false);
  const [remarksInput, setRemarksInput] = useState("");
  const notifications = useNotifications();
  const { t } = useTranslation();

  const handleAccept = async () => {
    setActionProgress(true);
    await props.onAccept(props.leaveRequest.leaveRequestId, remarksInput);
    setActionProgress(false);
  };
  const handleReject = async () => {
    setActionProgress(true);
    await props.onReject(props.leaveRequest.leaveRequestId, remarksInput);
    setActionProgress(false);
  };
  const handleCancel = async () => {
    setActionProgress(true);
    await props.onCancel(props.leaveRequest.leaveRequestId, remarksInput);
    setActionProgress(false);
  };
  const handleCopyToClipboard = () => {
    navigator.clipboard.writeText(
      `${window.location.origin}/details/${props.leaveRequest.leaveRequestId}`
    );
    notifications.show(t("Copied to clipboard"), {
      severity: "info",
      autoHideDuration: 3000,
    });
  };
  return (
    <Box sx={{ flexGrow: 1 }} margin={2}>
      <Grid container spacing={2}>
        <Grid size={11}>
          <Typography variant="h5">
            {props.leaveRequest.assignedTo.name}
          </Typography>
        </Grid>
        <Grid size={1}>
          <Button
            color="inherit"
            sx={{ minWidth: 0, padding: 0 }}
            onClick={handleCopyToClipboard}
          >
            <LinkIcon />
          </Button>
        </Grid>
        <Grid size={12}>
          <Divider />
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Request type</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={leaveTypeStyle}>
            {props.leaveType.name}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Leave from - to</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {props.leaveRequest.dateFrom} - {props.leaveRequest.dateTo}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Days</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {daysCounter.days(dateFrom, dateTo)}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Hours</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {DurationFormatter.format(props.leaveRequest.duration)}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Created date</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {createdDate.toLocaleString(formatDate)}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Status</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={leaveStatusStyle}>
            {props.leaveRequest.status}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Assigned to</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {props.leaveRequest.assignedTo.name}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Last modified by</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {props.leaveRequest.lastModifiedBy.name}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Last modified date</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {lastModifiedDate.toLocaleString(formatDate)}
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Remarks</Trans>
          </Typography>
        </Grid>
        <Grid size={6}>
          <Typography variant="body2" sx={defaultStyle}>
            {props.leaveRequest.remarks.map((x) => x.remarks).join(" | ")}
          </Typography>
        </Grid>
        <Grid
          size={12}
          sx={{
            display: "flex",
            justifyContent: "center",
            p: 1,
            m: 1,
          }}
        >
          <ButtonGroup variant="contained" aria-label="Basic button group">
            <Authorized roles={["DecisionMaker", "GlobalAdmin"]}>
              <LoadingButton
                loading={actionProgress}
                loadingPosition="start"
                startIcon={<ThumbUpIcon />}
                color="success"
                onClick={handleAccept}
              >
                <Trans>Accept</Trans>
              </LoadingButton>
              <LoadingButton
                loading={actionProgress}
                loadingPosition="start"
                startIcon={<ThumbDownIcon />}
                color="error"
                onClick={handleReject}
              >
                <Trans>Reject</Trans>
              </LoadingButton>
            </Authorized>
            {dateFrom > DateTime.local() &&
            props.leaveRequest.status !== "Canceled" &&
            props.leaveRequest.status !== "Rejected" ? (
              <Authorized
                roles={"CurrentUser"}
                userId={props.leaveRequest.assignedTo.id}
                authorized={
                  <LoadingButton
                    loading={actionProgress}
                    loadingPosition="start"
                    startIcon={<CancelIcon />}
                    color="warning"
                    onClick={handleCancel}
                  >
                    <Trans>Cancel</Trans>
                  </LoadingButton>
                }
              />
            ) : (
              <></>
            )}
          </ButtonGroup>
        </Grid>
        <Grid size={12}>
          <TextField
            id="remarks-multiline-static"
            label={t("Remarks")}
            multiline
            rows={1}
            variant="standard"
            sx={{ width: "100%" }}
            value={remarksInput}
            onChange={(e) => setRemarksInput(e.target.value)}
          />
        </Grid>
      </Grid>
    </Box>
  );
}
