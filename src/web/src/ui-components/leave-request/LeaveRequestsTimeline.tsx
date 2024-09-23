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
import { ShowData } from "../ShowData";
import { LeaveRequestsDto } from "./LeaveRequestsDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiData, setApiData] = useState<LeaveRequestsDto | null>(null);

  useEffect(() => {
    if (!apiData && inProgress === InteractionStatus.None) {
      callApi<LeaveRequestsDto>("/leaverequests?dateFrom=2024-08-21&dateTo=2024-12-23")
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
  const authRequest = {
    ...loginRequest,
  };

  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={authRequest}
      errorComponent={ErrorComponent}
      loadingComponent={Loading}
    >
      <DataContent />
    </MsalAuthenticationTemplate>
  );
}
