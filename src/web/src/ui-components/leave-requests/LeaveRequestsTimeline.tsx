import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import {
  InteractionStatus,
  InteractionType,
  InteractionRequiredAuthError,
  AccountInfo,
} from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApi } from "../../utils/ApiCall";
import ShowLeaveRequestsTimeline from "./ShowLeaveRequestsTimeline";
import { LeaveRequestsResponseDto } from "./LeaveRequestsDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiData, setApiData] = useState<LeaveRequestsResponseDto | null>(null);

  useEffect(() => {
    if (!apiData && inProgress === InteractionStatus.None) {
      callApi<LeaveRequestsResponseDto>("/leaverequests?dateFrom=2024-08-21&dateTo=2024-09-01")
        .then((response) => setApiData(response))
        .catch((e) => {
          if (e instanceof InteractionRequiredAuthError) {
            instance.acquireTokenRedirect({
              ...loginRequest,
              account: instance.getActiveAccount() as AccountInfo | undefined,
            });
          }
        });
    }
  }, [inProgress, apiData, instance]);

  return apiData ? <ShowLeaveRequestsTimeline {...apiData} /> : <Loading />;
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
