import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading, LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import ShowLeaveRequestDetails from "./ShowLeaveRequestDetails";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { useNotifications } from "@toolpad/core/useNotifications";

const DataContent = (props: {leaveRequestId: string}) => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequestDetails, setApiLeaveRequests] = useState<LeaveRequestDetailsDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] = useState<LeaveStatusesDto | null>(null);
  const [apiLeaveType, setApiLeaveType] = useState<LeaveTypeDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);
  const notifications = useNotifications();

  useEffect(() => {
    if (!apiLeaveRequestDetails && inProgress === InteractionStatus.None) {
      callApiGet<LeaveRequestDetailsDto>(`/leaverequests/${props.leaveRequestId}`, notifications.show)
        .then((response) => {
          callApiGet<LeaveTypeDto>(`/leavetypes/${response.leaveTypeId}`, notifications.show)
            .then((response) => setApiLeaveType(response))
            .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
          
          callApiGet<HolidaysDto>("/settings/holidays?dateFrom=2024-08-21&dateTo=2024-11-01", notifications.show)
            .then((response) => setApiHolidays(response))
            .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
          setApiLeaveRequests(response)
        })
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveStatusesDto>("/settings/leavestatus", notifications.show)
        .then((response) => setApiLeaveStatuses(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequestDetails, instance, props.leaveRequestId, notifications.show]);

  return apiLeaveRequestDetails && apiLeaveStatuses && apiLeaveType && apiHolidays ? (
    <ShowLeaveRequestDetails
      leaveRequest={apiLeaveRequestDetails}
      statusColor={apiLeaveStatuses.items.find(x => x.leaveRequestStatus === apiLeaveRequestDetails.status)?.color ?? "transparent"}
      leaveType={apiLeaveType}
      holidays={apiHolidays}
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
      loadingComponent={LoadingAuth}
    >
      <DataContent leaveRequestId={props.leaveRequestId}/>
    </MsalAuthenticationTemplate>
  );
}
