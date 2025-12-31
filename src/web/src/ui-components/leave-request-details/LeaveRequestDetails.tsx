import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";
import { Loading, LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { useApiQuery } from "../../hooks/useApiQuery";
import { useApiMutation } from "../../hooks/useApiMutation";
import { LeaveRequestDetailsDto } from "./LeaveRequestDetailsDto";
import ShowLeaveRequestDetails from "./ShowLeaveRequestDetails";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { useNotifications } from "@toolpad/core/useNotifications";
import { useNavigate } from "react-router-dom";
import Box from "@mui/material/Box";
import { useTranslation } from "react-i18next";

const DataContent = (props: {
  leaveRequestId: string;
  onClose?: () => void;
}) => {
  const { inProgress } = useMsal();
  const notifications = useNotifications();
  const navigate = useNavigate();
  const { t } = useTranslation();

  // Use TanStack Query for fetching leave request details
  const { data: apiLeaveRequestDetails } = useApiQuery<LeaveRequestDetailsDto>(
    ["leaveRequestDetails", props.leaveRequestId],
    `/leaverequests/${props.leaveRequestId}`,
    { enabled: inProgress === InteractionStatus.None && !!props.leaveRequestId }
  );

  // Fetch leave type based on leave request details
  const { data: apiLeaveType } = useApiQuery<LeaveTypeDto>(
    ["leaveType", apiLeaveRequestDetails?.leaveTypeId],
    `/leavetypes/${apiLeaveRequestDetails?.leaveTypeId}`,
    {
      enabled:
        inProgress === InteractionStatus.None &&
        !!apiLeaveRequestDetails?.leaveTypeId,
    }
  );

  // Fetch holidays based on leave request date range
  const { data: apiHolidays } = useApiQuery<HolidaysDto>(
    [
      "holidays",
      "details",
      apiLeaveRequestDetails?.dateFrom,
      apiLeaveRequestDetails?.dateTo,
    ],
    `/settings/holidays?dateFrom=${apiLeaveRequestDetails?.dateFrom}&dateTo=${apiLeaveRequestDetails?.dateTo}`,
    {
      enabled:
        inProgress === InteractionStatus.None &&
        !!apiLeaveRequestDetails?.dateFrom &&
        !!apiLeaveRequestDetails?.dateTo,
    }
  );

  // Fetch leave statuses
  const { data: apiLeaveStatuses } = useApiQuery<LeaveStatusesDto>(
    ["leaveStatuses"],
    "/settings/leavestatus",
    { enabled: inProgress === InteractionStatus.None }
  );

  // Create mutation for leave request actions
  const actionLeaveRequestMutation = useApiMutation<{ remark: string }>({
    onSuccess: (response) => {
      if (response.status === 200) {
        navigate("/");
        // TODO: Handle it better - invalidate queries instead of reload
        window.location.reload();
      }
    },
    invalidateQueries: [
      ["leaveRequestDetails", props.leaveRequestId],
      ["leaveRequests"],
    ],
  });

  async function handleActionLeaveRequest(
    id: string,
    remarks: string,
    action: "accept" | "reject" | "cancel",
    successMessage: string
  ) {
    actionLeaveRequestMutation.mutate(
      {
        url: `/leaverequests/${id}/${action}`,
        method: "PUT",
        body: { remark: remarks },
      },
      {
        onSuccess: () => {
          notifications.show(successMessage, {
            severity: "success",
            autoHideDuration: 3000,
          });
        },
      }
    );
  }

  const handleAccept = async (id: string, remarks: string) => {
    await handleActionLeaveRequest(
      id,
      remarks,
      "accept",
      t("Leave Request was accepted successfully")
    );
  };
  const handleReject = async (id: string, remarks: string) => {
    await handleActionLeaveRequest(
      id,
      remarks,
      "reject",
      t("Leave Request was rejected successfully")
    );
  };
  const handleCancel = async (id: string, remarks: string) => {
    await handleActionLeaveRequest(
      id,
      remarks,
      "cancel",
      t("Leave Request was cancelled successfully")
    );
  };

  return apiLeaveRequestDetails &&
    apiLeaveStatuses &&
    apiLeaveType &&
    apiHolidays ? (
    <ShowLeaveRequestDetails
      leaveRequest={apiLeaveRequestDetails}
      statusColor={
        apiLeaveStatuses.items.find(
          (x) => x.leaveRequestStatus === apiLeaveRequestDetails.status
        )?.color ?? "transparent"
      }
      leaveType={apiLeaveType}
      holidays={apiHolidays}
      onAccept={handleAccept}
      onReject={handleReject}
      onCancel={handleCancel}
      onClose={props.onClose}
    />
  ) : (
    <Box
      sx={{
        minWidth: 170,
        minHeight: 170,
        textAlign: "center",
        alignContent: "center",
      }}
    >
      <Loading />
    </Box>
  );
};

export function LeaveRequestDetails(
  props: Readonly<{ leaveRequestId: string; onClose?: () => void }>
) {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingAuth}
    >
      <DataContent
        leaveRequestId={props.leaveRequestId}
        onClose={props.onClose}
      />
    </MsalAuthenticationTemplate>
  );
}
