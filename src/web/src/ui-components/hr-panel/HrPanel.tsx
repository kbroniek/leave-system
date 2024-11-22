import { InteractionStatus, InteractionType } from "@azure/msal-browser"
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react"
import { LoadingAuth } from "../../components/Loading"
import { ErrorComponent } from "../../components/ErrorComponent"
import { Authorized } from "../../components/Authorized"
import { Forbidden } from "../../components/Forbidden"
import { loginRequest } from "../../authConfig";
import { useEffect, useState } from "react"
import { ShowHrPanel } from "./ShowHrPanel"
import { TextField } from "@mui/material"
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto"
import { LeaveTypesDto } from "../dtos/LeaveTypesDto"
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto"
import { useSearchParams } from "react-router-dom"
import { useNotifications } from "@toolpad/core"
import { DateTime } from "luxon"
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall"

const DataContent = (): JSX.Element => {
    
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] =
    useState<LeaveRequestsResponseDto | null>(null);
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
      if (!apiLeaveTypes) {
        callApiGet<LeaveTypesDto>("/leavetypes", notifications.show)
          .then((response) => setApiLeaveTypes(response))
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
  ]);
  return (
    <>
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
      <ShowHrPanel
        leaveRequests={apiLeaveRequests?.items}
        leaveTypes={apiLeaveTypes?.items}
        leaveLimits={apiLeaveLimits?.items}
      />
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