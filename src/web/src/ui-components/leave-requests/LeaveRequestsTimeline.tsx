import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import {
  InteractionStatus,
  InteractionType
} from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApi, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import ShowLeaveRequestsTimeline from "./ShowLeaveRequestsTimeline";
import { LeaveRequestsResponseDto } from "./LeaveRequestsDto";
import { HolidaysDto } from "./HolidaysDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiData, setApiData] = useState<LeaveRequestsResponseDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);

  useEffect(() => {
    if (!apiData && inProgress === InteractionStatus.None) {
      callApi<LeaveRequestsResponseDto>("/leaverequests?dateFrom=2024-08-21&dateTo=2024-11-01")
        .then((response) => setApiData(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
        callApi<HolidaysDto>("/settings/holidays?dateFrom=2024-08-21&dateTo=2024-11-01")
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiData, instance]);

  return apiData && apiHolidays ? <ShowLeaveRequestsTimeline leaveRequests={apiData} holidays={apiHolidays} /> : <Loading />;
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
