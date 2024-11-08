import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading, LoadingAuth } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import ShowLeaveRequestsTimeline from "./ShowLeaveRequestsTimeline";
import { LeaveRequestsResponseDto } from "./LeaveRequestsDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] = useState<LeaveRequestsResponseDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] = useState<LeaveStatusesDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(null);

  const dateFrom = "2024-08-21";
  const dateTo = "2024-11-30";

  useEffect(() => {
    if (!apiLeaveRequests && inProgress === InteractionStatus.None) {
      callApiGet<LeaveRequestsResponseDto>(`/leaverequests?dateFrom=${dateFrom}&dateTo=${dateTo}`)
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<HolidaysDto>(`/settings/holidays?dateFrom=${dateFrom}&dateTo=${dateTo}`)
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveStatusesDto>("/settings/leavestatus")
        .then((response) => setApiLeaveStatuses(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveTypesDto>("/leavetypes")
        .then((response) => setApiLeaveTypes(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequests, instance]);

  return apiLeaveRequests && apiHolidays && apiLeaveStatuses && apiLeaveTypes ? (
    <ShowLeaveRequestsTimeline
      leaveRequests={apiLeaveRequests}
      holidays={apiHolidays}
      leaveStatuses={apiLeaveStatuses}
      leaveTypes={apiLeaveTypes}
    />
  ) : (
    <Loading />
  );
};

export function LeaveRequestsTimeline() {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingAuth}
    >
      <DataContent />
    </MsalAuthenticationTemplate>
  );
}
