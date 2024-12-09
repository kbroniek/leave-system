import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading, LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { callApi, callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import ShowLeaveRequestDetails from "./ShowLeaveRequestDetails";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { useNotifications } from "@toolpad/core/useNotifications";
import { useNavigate } from "react-router-dom";

const DataContent = (props: {leaveRequestId: string}) => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequestDetails, setApiLeaveRequestDetails] = useState<LeaveRequestDetailsDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] = useState<LeaveStatusesDto | null>(null);
  const [apiLeaveType, setApiLeaveType] = useState<LeaveTypeDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);
  const [isCallApi, setIsCallApi] = useState(true);
  const notifications = useNotifications();
  const navigate = useNavigate();

  async function handleActionLeaveRequest(id: string, remarks: string, action: "accept" | "reject" | "cancel", successMessage: string) {
    const body = {
      remark: remarks,
    };
    const response = await callApi(`/leaverequests/${id}/${action}`, "PUT", body, notifications.show);
    if (response.status === 200) {
      notifications.show(successMessage, {
        severity: "success",
        autoHideDuration: 3000,
      });
      navigate("/");
      //TOD: Handle it better
      window.location.reload();
    }
  }
  const handleAccept = async (id: string, remarks: string) => {
    await handleActionLeaveRequest(id, remarks, "accept", "Leave Request was accepted successfully");
  }
  const handleReject = async (id: string, remarks: string) => {
    await handleActionLeaveRequest(id, remarks, "reject", "Leave Request was rejected successfully");
  }
  const handleCancel = async (id: string, remarks: string) => {
    await handleActionLeaveRequest(id, remarks, "cancel", "Leave Request was cancelled successfully");
  }

  useEffect(() => {
    if (isCallApi && inProgress === InteractionStatus.None) {
      setIsCallApi(false);
      callApiGet<LeaveRequestDetailsDto>(`/leaverequests/${props.leaveRequestId}`, notifications.show)
        .then((response) => {
          callApiGet<LeaveTypeDto>(`/leavetypes/${response.leaveTypeId}`, notifications.show)
            .then((response) => setApiLeaveType(response))
            .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
          callApiGet<HolidaysDto>("/settings/holidays?dateFrom=2024-08-21&dateTo=2024-11-01", notifications.show)
            .then((response) => setApiHolidays(response))
            .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
          setApiLeaveRequestDetails(response)
        })
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveStatusesDto>("/settings/leavestatus", notifications.show)
        .then((response) => setApiLeaveStatuses(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, isCallApi, instance, props.leaveRequestId, notifications.show]);

  return apiLeaveRequestDetails && apiLeaveStatuses && apiLeaveType && apiHolidays ? (
    <ShowLeaveRequestDetails
      leaveRequest={apiLeaveRequestDetails}
      statusColor={apiLeaveStatuses.items.find(x => x.leaveRequestStatus === apiLeaveRequestDetails.status)?.color ?? "transparent"}
      leaveType={apiLeaveType}
      holidays={apiHolidays}
      onAccept={handleAccept}
      onReject={handleReject}
      onCancel={handleCancel}
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
