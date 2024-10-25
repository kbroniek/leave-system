import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApi, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveRequestsResponseDto } from "../leave-requests/LeaveRequestsDto";
import { SubmitLeaveRequestForm } from "./SubmitLeaveRequestForm";
import { DateTime } from "luxon";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] =
    useState<LeaveRequestsResponseDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(null);
  const [apiLeaveLimits, setApiLeaveLimits] = useState<LeaveLimitsDto | null>(null);

  useEffect(() => {
    if (!apiLeaveRequests && inProgress === InteractionStatus.None) {
      const userId = instance.getActiveAccount()?.idTokenClaims?.sub;
      const now = DateTime.now();
      const today = now.toFormat("yyyy-MM-dd");
      const currentYear = now.toFormat("yyyy");
      callApi<LeaveRequestsResponseDto>(`/leaverequests?dateFrom=${today}&dateTo=${today}&AssignedToUserIds=${userId}`)
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<HolidaysDto>(`/settings/holidays?dateFrom=${today}&dateTo=${today}`)
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<LeaveTypesDto>("/leavetypes")
        .then((response) => setApiLeaveTypes(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<LeaveLimitsDto>(`/leavelimits/user?year=${currentYear}`)
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequests, instance]);

  return apiLeaveRequests && apiHolidays && apiLeaveTypes && apiLeaveLimits ? (
    <SubmitLeaveRequestForm
      leaveRequests={apiLeaveRequests}
      holidays={apiHolidays}
      leaveTypes={apiLeaveTypes.items}
      leaveLimits={apiLeaveLimits?.items}
    />
  ) : (
    <Loading />
  );
};

export function SubmitLeaveRequest() {
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
