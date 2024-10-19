import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApi, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import ShowLeaveRequestDetails from "./ShowLeaveRequestDetails";

const DataContent = (props: {leaveRequestId: string}) => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequestDetails, setApiLeaveRequests] = useState<LeaveRequestDetailsDto | null>(null);

  useEffect(() => {
    if (!apiLeaveRequestDetails && inProgress === InteractionStatus.None) {
      callApi<LeaveRequestDetailsDto>(`/leaverequests/${props.leaveRequestId}`)
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequestDetails, instance, props.leaveRequestId]);

  return apiLeaveRequestDetails  ? (
    <ShowLeaveRequestDetails
      leaveRequest={apiLeaveRequestDetails}
    />
  ) : (
    <Loading />
  );
};

export function LeaveRequestDetails(leaveRequestId: string) {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={Loading}
    >
      <DataContent leaveRequestId={leaveRequestId}/>
    </MsalAuthenticationTemplate>
  );
}
