import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { useState } from "react";
import { ErrorComponent } from "../../components/ErrorComponent";
import { LoadingAuth } from "../../components/Loading";
import { loginRequest } from "../../authConfig";
import { DateTime } from "luxon";
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto";
import { useSearchParams } from "react-router-dom";
import { useApiQuery } from "../../hooks/useApiQuery";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { ShowMyLeaveRequests } from "./ShowMyLeaveRequests";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import TextField from "@mui/material/TextField";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Grid";
import Typography from "@mui/material/Typography";
import { leaveRequestsStatuses } from "../utils/Status";

const DataContent = (params: { userId?: string }) => {
  const { instance, inProgress } = useMsal();
  const [searchParams, setSearchParams] = useSearchParams();
  const queryYear = Number(searchParams.get("year"));
  const [currentYear, setCurrentYear] = useState<number>(
    !queryYear ? DateTime.local().year : queryYear
  );
  
  const userId = params.userId ?? instance.getActiveAccount()?.idTokenClaims?.oid;
  const now = DateTime.fromObject({ year: currentYear });
  const dateFromFormatted = now.startOf("year").toFormat("yyyy-MM-dd");
  const dateToFormatted = now.endOf("year").toFormat("yyyy-MM-dd");
  const leaveRequestsUrl = `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}&assignedToUserIds=${userId}${leaveRequestsStatuses
    .map((x) => `&statuses=${x}`)
    .join("")}`;

  // Use TanStack Query for all API calls
  const { data: apiLeaveRequests } = useApiQuery<LeaveRequestsResponseDto>(
    ["leaveRequests", userId, currentYear],
    leaveRequestsUrl,
    { enabled: inProgress === InteractionStatus.None && !!userId }
  );

  const { data: apiLeaveLimits } = useApiQuery<LeaveLimitsDto>(
    ["leaveLimits", userId, currentYear],
    params.userId 
      ? `/leavelimits?year=${currentYear}&userIds=${userId}`
      : `/leavelimits/user?year=${currentYear}`,
    { enabled: inProgress === InteractionStatus.None && !!userId }
  );

  const { data: apiLeaveTypes } = useApiQuery<LeaveTypesDto>(
    ["leaveTypes"],
    "/leavetypes",
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiLeaveStatuses } = useApiQuery<LeaveStatusesDto>(
    ["leaveStatuses"],
    "/settings/leavestatus",
    { enabled: inProgress === InteractionStatus.None }
  );
  return (
    <>
      <Paper elevation={3} sx={{ margin: "3px 0", width: "100%", padding: 1 }}>
        <Grid container spacing={0} sx={{ justifyContent: "center" }}>
          <Typography sx={{ alignContent: "center", padding: 2 }}>
            Year
          </Typography>
          <TextField
            type="number"
            value={currentYear}
            onChange={(v) => {
              const value = Number(v.target.value);
              if (value !== currentYear) {
                setCurrentYear(value);
                setSearchParams({ year: v.target.value });
              }
            }}
          />
        </Grid>
      </Paper>
      <Paper elevation={3} sx={{ margin: "3px 0", width: "100%" }}>
        <ShowMyLeaveRequests
          leaveRequests={apiLeaveRequests?.items}
          leaveStatuses={apiLeaveStatuses?.items}
          leaveTypes={apiLeaveTypes?.items}
          leaveLimits={apiLeaveLimits?.items}
        />
      </Paper>
    </>
  );
};

export function MyLeaveRequests(params: { userId?: string }) {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingAuth}
    >
      <Authorized
        roles={["Employee", "HumanResource", "GlobalAdmin"]}
        authorized={<DataContent userId={params.userId} />}
        unauthorized={<Forbidden />}
      />
    </MsalAuthenticationTemplate>
  );
}
