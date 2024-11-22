import { InteractionStatus, InteractionType } from "@azure/msal-browser"
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react"
import { LoadingAuth } from "../../components/Loading"
import { ErrorComponent } from "../../components/ErrorComponent"
import { Authorized } from "../../components/Authorized"
import { Forbidden } from "../../components/Forbidden"
import { loginRequest } from "../../authConfig";
import { useEffect, useState } from "react"
import { ShowHrPanel } from "./ShowHrPanel"
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto"
import { LeaveTypesDto } from "../dtos/LeaveTypesDto"
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto"
import { useSearchParams } from "react-router-dom"
import { useNotifications } from "@toolpad/core"
import { DateTime } from "luxon"
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall"
import { HolidaysDto } from "../dtos/HolidaysDto"
import { EmployeesDto } from "../dtos/EmployeesDto"
import Paper from "@mui/material/Paper"
import TextField from "@mui/material/TextField"
import Typography from "@mui/material/Typography"
import Grid from "@mui/material/Grid2"

const DataContent = (): JSX.Element => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] =
    useState<LeaveRequestsResponseDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(
    null,
  );
  const [apiLeaveLimits, setApiLeaveLimits] = useState<LeaveLimitsDto | null>();
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | undefined>();
  const [apiEmployees, setApiEmployees] = useState<EmployeesDto | undefined>();
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
      const now = DateTime.fromObject({ year: currentYear });
      const dateFromFormatted = now.startOf("year").toFormat("yyyy-MM-dd");
      const dateToFormatted = now.endOf("year").toFormat("yyyy-MM-dd");
      callApiGet<LeaveRequestsResponseDto>(
        `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`,
        notifications.show,
      )
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveLimitsDto>(
        `/leavelimits?year=${currentYear}`,
        notifications.show,
      )
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));

      callApiGet<HolidaysDto>(
        `/settings/holidays?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`,
        notifications.show,
      )
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      if (!apiLeaveTypes) {
        callApiGet<LeaveTypesDto>("/leavetypes", notifications.show)
          .then((response) => setApiLeaveTypes(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      }
      if(!apiEmployees) {
        callApiGet<EmployeesDto>("/employees", notifications.show)
          .then((response) => setApiEmployees(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      }
    }
  }, [
    inProgress,
    instance,
    notifications.show,
    apiLeaveTypes,
    currentYear,
    isCallApi,
    apiEmployees,
  ]);
  return (
    <>
    <Paper elevation={3} sx={{ margin: "3px 0", width: "100%", padding: 1 }}>
      <Grid container spacing={0} sx={{justifyContent: "center"}}>
        <Typography sx={{alignContent: "center", padding: 2}}>Year</Typography>
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
      <ShowHrPanel
        leaveRequests={apiLeaveRequests?.items}
        leaveTypes={apiLeaveTypes?.items}
        leaveLimits={apiLeaveLimits?.items}
        employees={apiEmployees?.items}
        holidays={apiHolidays?.items}

      />
    </Paper>
    </>
  );
}
export const HrPanel = (): JSX.Element => <MsalAuthenticationTemplate
    interactionType={InteractionType.Redirect}
    authenticationRequest={loginRequest}
    errorComponent={ErrorComponent}
    loadingComponent={LoadingAuth}
>
    <Authorized
        roles={["GlobalAdmin", "HumanResource"]}
        authorized={<DataContent />}
        unauthorized={<Forbidden />} />
</MsalAuthenticationTemplate>