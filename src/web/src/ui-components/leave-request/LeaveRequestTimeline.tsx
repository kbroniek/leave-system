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
import { ShowData } from "../ShowData";
import { Loading } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApi } from "../../utils/ApiCall";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiData, setApiData] = useState(null);

  useEffect(() => {
    if (!apiData && inProgress === InteractionStatus.None) {
      callApi("/leaverequests")
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

  return apiData ? <ShowData apiData={apiData} /> : <Loading />;
};

export function LeaveRequestTimeline() {
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
