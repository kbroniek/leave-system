import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApi, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import ShowLeaveRequestsTimeline from "./ShowLeaveRequestsTimeline";
import { LeaveRequestsResponseDto } from "./LeaveRequestsDto";
import { HolidaysDto } from "./HolidaysDto";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] = useState<LeaveRequestsResponseDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] = useState<LeaveStatusesDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(null);

  useEffect(() => {
    if (!apiLeaveRequests && inProgress === InteractionStatus.None) {
      callApi<LeaveRequestsResponseDto>("/leaverequests?dateFrom=2024-08-21&dateTo=2024-11-01")
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<HolidaysDto>("/settings/holidays?dateFrom=2024-08-21&dateTo=2024-11-01")
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<LeaveStatusesDto>("/settings/leavestatus")
        .then((response) => setApiLeaveStatuses(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<LeaveTypesDto>("/leavetypes")
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
      loadingComponent={Loading}
    >
      <DataContent />
    </MsalAuthenticationTemplate>
  );
}
