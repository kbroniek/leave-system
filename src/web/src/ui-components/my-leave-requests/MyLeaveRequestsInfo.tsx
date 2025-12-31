import Typography from "@mui/material/Typography";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import Box from "@mui/material/Box";
import Grid from "@mui/material/Grid";
import Divider from "@mui/material/Divider";
import { CircularProgress } from "@mui/material";
import { Duration } from "luxon";
import { DurationFormatter } from "../utils/DurationFormatter";
import { LeaveRequestsUsed } from "./LeaveRequestsUsed";
import { filterLeaveRequests, findLeaveTypes as findLeaveType } from "./Utils";
import React from "react";
import { LimitsCalculator } from "../utils/LimitsCalculator";
import { Trans, useTranslation } from "react-i18next";

const defaultStyle = { paddingTop: "2px", textAlign: "right" };
const titleStyle = { color: "text.secondary" };

export const MyLeaveRequestsInfo = (params: {
  leaveRequests: LeaveRequestDto[] | undefined;
  leaveTypes: LeaveTypeDto[] | undefined;
  leaveLimits: LeaveLimitDto[] | undefined;
}): React.ReactElement => {
  const { t } = useTranslation();
  const holidayLeaveType = findLeaveType(params.leaveTypes, "Holiday");
  const onDemandLeaveType = findLeaveType(params.leaveTypes, "OnDemand");
  const holidayLeaveRequests = filterLeaveRequests(params.leaveRequests, [
    holidayLeaveType?.id,
    onDemandLeaveType?.id,
  ]);
  const holidayDuration = holidayLeaveRequests
    ? LimitsCalculator.calculateTotalDuration(
        ...holidayLeaveRequests.map((x) => x.duration)
      )
    : undefined;
  const holidayLimits =
    params.leaveLimits?.filter((x) => x.leaveTypeId === holidayLeaveType?.id) ??
    [];
  const { limit, overdueLimit, totalLimit } = LimitsCalculator.calculateLimits(
    ...holidayLimits
  );
  const leaveTypesLeft = params.leaveTypes?.filter(
    (x) =>
      x.properties?.catalog !== "Holiday" &&
      x.properties?.catalog !== "Saturday" &&
      x.properties?.catalog !== "OnDemand"
  );
  return (
    <Box sx={{ flexGrow: 1 }} margin={2}>
      <Grid container spacing={2}>
        <Grid size={12}>
          <Typography variant="h5">Information</Typography>
          <Divider />
        </Grid>
        <Grid size={11}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Used</Trans>
          </Typography>
        </Grid>
        <Grid size={1}>
          <TotalDuration
            isLoading={!params.leaveTypes || !params.leaveRequests}
            duration={holidayDuration}
            workingHours={holidayLeaveRequests?.find(() => true)?.workingHours}
          />
        </Grid>
        <Grid size={11}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Limit</Trans>
          </Typography>
        </Grid>
        <Grid size={1}>
          <TotalDuration
            isLoading={!params.leaveTypes || !params.leaveLimits}
            duration={limit}
            workingHours={holidayLimits.find(() => true)?.workingHours}
          />
        </Grid>
        <Grid size={11}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Overdue limit</Trans>
          </Typography>
        </Grid>
        <Grid size={1}>
          <TotalDuration
            isLoading={!params.leaveTypes || !params.leaveLimits}
            duration={overdueLimit}
            workingHours={holidayLimits.find(() => true)?.workingHours}
          />
        </Grid>
        <Grid size={11}>
          <Typography variant="body1" sx={titleStyle}>
            <Trans>Left</Trans>
          </Typography>
        </Grid>
        <Grid size={1}>
          <TotalDuration
            isLoading={
              !params.leaveTypes || !params.leaveLimits || !params.leaveRequests
            }
            duration={
              holidayDuration ? totalLimit?.minus(holidayDuration) : undefined
            }
            workingHours={holidayLimits.find(() => true)?.workingHours}
          />
        </Grid>
        <LeaveRequestsUsed
          leaveRequests={params.leaveRequests}
          leaveLimits={params.leaveLimits}
          leaveTypes={params.leaveTypes}
          text={t("On demand")}
          leaveTypeCatalog="OnDemand"
        />
        <LeaveRequestsUsed
          leaveRequests={params.leaveRequests}
          leaveLimits={params.leaveLimits}
          leaveTypes={params.leaveTypes}
          text={t("Free Saturday")}
          leaveTypeCatalog="Saturday"
        />
        <Grid size={12}>
          <Divider />
        </Grid>
        {leaveTypesLeft?.map((x) => {
          const leaveRequestPerLeaveType = params.leaveRequests?.filter(
            (lr) => lr.leaveTypeId === x.id
          );
          const duration = leaveRequestPerLeaveType
            ? LimitsCalculator.calculateTotalDuration(
                ...leaveRequestPerLeaveType.map((lr) => lr.duration)
              )
            : undefined;
          return (
            <React.Fragment key={`my-leave-types-${x.id}`}>
              <Grid size={11}>
                <Typography variant="body1" sx={titleStyle}>
                  {x.name}
                </Typography>
              </Grid>
              <Grid size={1}>
                <TotalDuration
                  isLoading={!params.leaveTypes || !params.leaveRequests}
                  duration={duration}
                  workingHours={
                    leaveRequestPerLeaveType?.find(() => true)?.workingHours
                  }
                />
              </Grid>
            </React.Fragment>
          );
        })}
      </Grid>
    </Box>
  );
};

function TotalDuration(params: {
  isLoading: boolean;
  duration: Duration | undefined;
  workingHours: string | undefined;
}): React.ReactElement {
  return params.isLoading ? (
    <CircularProgress size="20px" />
  ) : (
    <Typography variant="body2" sx={defaultStyle}>
      {params.duration
        ? DurationFormatter.format(params.duration, params.workingHours)
        : "-"}
    </Typography>
  );
}
