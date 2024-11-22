import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { useEffect, useState } from "react";
import { ErrorComponent } from "../../components/ErrorComponent";
import { LoadingAuth } from "../../components/Loading";
import { loginRequest } from "../../authConfig";
import { DateTime } from "luxon";
import { useNotifications } from "@toolpad/core/useNotifications";
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto";
import { useSearchParams } from "react-router-dom";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { ShowMyLeaveRequests } from "./ShowMyLeaveRequests";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import TextField from "@mui/material/TextField";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Grid2";
import Typography from "@mui/material/Typography";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] =
    useState<LeaveRequestsResponseDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] =
    useState<LeaveStatusesDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(
    null,
  );
  const [apiLeaveLimits, setApiLeaveLimits] = useState<LeaveLimitsDto | null>();
  const [searchParams, setSearchParams] = useSearchParams();
  const notifications = useNotifications();
  const queryYear = Number(searchParams.get("year"));
  const [currentYear, setCurrentYear] = useState<number>(
    !queryYear ? DateTime.local().year : queryYear,
  );
  const [isCallApi, setIsCallApi] = useState(true);

  useEffect(() => {
    if (isCallApi && inProgress === InteractionStatus.None) {
      setIsCallApi(false);
      const userId = instance.getActiveAccount()?.idTokenClaims?.sub;
      const now = DateTime.fromObject({ year: currentYear });
      const dateFromFormatted = now.startOf("year").toFormat("yyyy-MM-dd");
      const dateToFormatted = now.endOf("year").toFormat("yyyy-MM-dd");
      const leaveRequestsStatuses = [
        "Init",
        "Pending",
        "Accepted",
        "Canceled",
        "Rejected",
        "Deprecated",
      ];
      callApiGet<LeaveRequestsResponseDto>(
        `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}&assignedToUserIds=${userId}${leaveRequestsStatuses.map((x) => `&statuses=${x}`).join("")}`,
        notifications.show,
      )
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveLimitsDto>(
        `/leavelimits/user?year=${currentYear}`,
        notifications.show,
      )
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      if (!apiLeaveTypes) {
        callApiGet<LeaveTypesDto>("/leavetypes", notifications.show)
          .then((response) => setApiLeaveTypes(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      }
      if (!apiLeaveStatuses) {
        callApiGet<LeaveStatusesDto>(
          "/settings/leavestatus",
          notifications.show,
        )
          .then((response) => setApiLeaveStatuses(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      }
    }
  }, [
    inProgress,
    instance,
    notifications.show,
    apiLeaveTypes,
    apiLeaveStatuses,
    currentYear,
    isCallApi,
  ]);
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
                setApiLeaveRequests(null);
                setApiLeaveLimits(null);
                setIsCallApi(true);
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

export function MyLeaveRequests() {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingAuth}
    >
      <Authorized
        roles={["Employee"]}
        authorized={<DataContent />}
        unauthorized={<Forbidden />}
      />
    </MsalAuthenticationTemplate>
  );
}
