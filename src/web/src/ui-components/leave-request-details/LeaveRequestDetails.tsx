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
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";

const DataContent = (props: {leaveRequestId: string}) => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequestDetails, setApiLeaveRequests] = useState<LeaveRequestDetailsDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] = useState<LeaveStatusesDto | null>(null);
  const [apiLeaveType, setApiLeaveType] = useState<LeaveTypeDto | null>(null);

  useEffect(() => {
    if (!apiLeaveRequestDetails && inProgress === InteractionStatus.None) {
      callApi<LeaveRequestDetailsDto>(`/leaverequests/${props.leaveRequestId}`)
        .then((response) => {
          callApi<LeaveTypeDto>(`/leavetypes/${response.leaveTypeId}`)
            .then((response) => setApiLeaveType(response))
            .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
          setApiLeaveRequests(response)
        })
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<LeaveStatusesDto>("/settings/leavestatus")
        .then((response) => setApiLeaveStatuses(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequestDetails, instance, props.leaveRequestId]);

  return apiLeaveRequestDetails && apiLeaveStatuses && apiLeaveType ? (
    <ShowLeaveRequestDetails
      leaveRequest={apiLeaveRequestDetails}
      statusColor={apiLeaveStatuses.items.find(x => x.leaveRequestStatus === apiLeaveRequestDetails.status)?.color ?? "transparent"}
      leaveType={apiLeaveType}
    />
  ) : (
    <Loading />
  );
};

export function LeaveRequestDetails(props: {leaveRequestId: string}) {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={Loading}
    >
      <DataContent leaveRequestId={props.leaveRequestId}/>
    </MsalAuthenticationTemplate>
  );
}
