import Grid from "@mui/material/Grid2";
import Typography from "@mui/material/Typography";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeCatalog, LeaveTypeDto } from "../dtos/LeaveTypesDto";
import CircularProgress from "@mui/material/CircularProgress";
import { DateTime, Duration } from "luxon";
import {
  filterLeaveRequests,
} from "./Utils";
import React from "react";
import { LimitsCalculator } from "../utils/LimitsCalculator";

const defaultStyle = { paddingTop: "2px", textAlign: "right" };
const titleStyle = { color: "text.secondary" };

export const LeaveRequestsUsed = (params: {
  leaveRequests: LeaveRequestDto[] | undefined;
  leaveTypes: LeaveTypeDto[] | undefined;
  leaveLimits: LeaveLimitDto[] | undefined;
  text: string;
  leaveTypeCatalog: LeaveTypeCatalog;
}): JSX.Element => {
  const leaveType = findLeaveType(params.leaveTypes, params.leaveTypeCatalog);
  const limits = filterLimits(params.leaveLimits, leaveType?.id);
  const leaveRequestsFound = filterLeaveRequests(
    params.leaveRequests,
    leaveType?.id,
  );
  const daysLimit = calculateLimitDays(limits) ?? 0;
  const daysUsed = calculateLeaveRequestDays(leaveRequestsFound) ?? 0;
  return (
    <>
      <Grid size={11}>
        <Typography variant="body1" sx={titleStyle}>
          {params.text}
        </Typography>
      </Grid>
      <Grid size={1}>
        <TextFieldLoading
          isLoading={
            !params.leaveTypes || !params.leaveLimits || !params.leaveRequests
          }
          text={`${daysUsed}/${daysLimit}`}
        />
      </Grid>
      {limits
        ?.filter((x) => !!x.description)
        .map((x) => {
          const leaveRequestsUsed = leaveRequestsFound?.filter(
            (lr) =>
              (!x.validSince ||
                DateTime.fromISO(x.validSince) <=
                  DateTime.fromISO(lr.dateFrom)) &&
              (!x.validUntil ||
                DateTime.fromISO(x.validUntil) >= DateTime.fromISO(lr.dateTo)),
          );
          if (leaveRequestsUsed?.length === 0) {
            return (
              <React.Fragment key={"my-leave-requests-not-used"}>
                <Grid size={10}>
                  <Typography variant="body1">{x.description}</Typography>
                </Grid>
                <Grid size={2}>
                  <TextFieldLoading
                    isLoading={
                      !params.leaveTypes ||
                      !params.leaveLimits ||
                      !params.leaveRequests
                    }
                    text={"Not used"}
                  />
                </Grid>
              </React.Fragment>
            );
          }
          return leaveRequestsUsed?.map((u) => {
            return (
              <React.Fragment key={`my-leave-requests-${u.id}`}>
                <Grid size={9}>
                  <Typography variant="body1">{x.description}</Typography>
                </Grid>
                <Grid size={3}>
                  <TextFieldLoading
                    isLoading={
                      !params.leaveTypes ||
                      !params.leaveLimits ||
                      !params.leaveRequests
                    }
                    text={`Used (${u.dateFrom})`}
                  />
                </Grid>
              </React.Fragment>
            );
          });
        })}
    </>
  );
};

function TextFieldLoading(params: {
  isLoading: boolean;
  text: string;
}): JSX.Element {
  return params.isLoading ? (
    <CircularProgress size="20px" />
  ) : (
    <Typography variant="body2" sx={defaultStyle}>
      {params.text}
    </Typography>
  );
}

function filterLimits(
  leaveLimits: LeaveLimitDto[] | undefined,
  leaveTypeId: string | undefined,
) {
  return leaveLimits?.filter((x) => x.leaveTypeId === leaveTypeId);
}

function findLeaveType(
  leaveTypes: LeaveTypeDto[] | undefined,
  leaveTypeCatalog: LeaveTypeCatalog,
) {
  return leaveTypes?.find((x) => x.properties?.catalog === leaveTypeCatalog);
}

function calculateLimitDays(
  onDemandLimits: LeaveLimitDto[] | undefined,
): number | undefined {
  if (!onDemandLimits) {
    return;
  }
  const { totalLimit } = LimitsCalculator.calculateLimits(...onDemandLimits);
  const workingHoursDuration = getDuration(
    onDemandLimits.find(() => true)?.workingHours,
  );
  if (!workingHoursDuration || !totalLimit) {
    return;
  }
  return calculateDays(totalLimit, workingHoursDuration);
}
function calculateDays(
  totalLimit: Duration<boolean>,
  workingHoursDuration: Duration<boolean>,
): number {
  return Math.round(totalLimit.as("hours") / workingHoursDuration.as("hours"));
}

function calculateLeaveRequestDays(
  leaveRequests: LeaveRequestDto[] | undefined,
): number | undefined {
  if (!leaveRequests) {
    return;
  }
  const duration = LimitsCalculator.calculateTotalDuration(
    ...leaveRequests.map((x) => x.duration),
  );
  const workingHoursDuration = getDuration(
    leaveRequests.find(() => true)?.workingHours,
  );
  if (!workingHoursDuration || !duration) {
    return;
  }
  return calculateDays(duration, workingHoursDuration);
}
function getDuration(workingHours: string | undefined): Duration | undefined {
  const workingHoursDuration = Duration.fromISO(workingHours ?? "");
  const workingHoursValid = workingHoursDuration.isValid
    ? workingHoursDuration
    : undefined;
  return workingHoursValid;
}
