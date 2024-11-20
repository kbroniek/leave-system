import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { useEffect, useState } from "react";
import { ErrorComponent } from "../../components/ErrorComponent";
import { LoadingAuth } from "../Loading";
import { loginRequest } from "../../authConfig";
import { DateTime } from "luxon";
import { useNotifications } from "@toolpad/core/useNotifications";
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto";
import { useSearchParams } from "react-router-dom";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { ShowMyLeaveRequests } from "./ShowMyLeaveRequests";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] =
    useState<LeaveRequestsResponseDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] =
    useState<LeaveStatusesDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(
    null,
  );
  const [apiLeaveLimits, setApiLeaveLimits] = useState<
    LeaveLimitsDto | undefined
  >();
  const [searchParams] = useSearchParams();
  const notifications = useNotifications();

  useEffect(() => {
    if (!apiLeaveRequests && inProgress === InteractionStatus.None) {
      const userId = instance.getActiveAccount()?.idTokenClaims?.sub;
      const queryYear = Number(searchParams.get("year"));
      const currentYear = !queryYear ? DateTime.local().year : queryYear;
      const now = DateTime.fromObject({ year: currentYear });
      const dateFromFormatted = now.startOf("year").toFormat("yyyy-MM-dd");
      const dateToFormatted = now.endOf("year").toFormat("yyyy-MM-dd");
      
      callApiGet<LeaveRequestsResponseDto>(
        `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}&AssignedToUserIds=${userId}`,
        notifications.show,
      )
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      if(!apiLeaveTypes) {
        callApiGet<LeaveTypesDto>("/leavetypes", notifications.show)
          .then((response) => setApiLeaveTypes(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      }
      if(!apiLeaveStatuses) {
        callApiGet<LeaveStatusesDto>("/settings/leavestatus", notifications.show)
          .then((response) => setApiLeaveStatuses(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      }
      callApiGet<LeaveLimitsDto>(
        `/leavelimits/user?year=${currentYear}`,
        notifications.show,
      )
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequests, instance, notifications.show, apiLeaveTypes, apiLeaveStatuses, searchParams]);
  return <ShowMyLeaveRequests
    leaveRequests={apiLeaveRequests?.items}
    leaveStatuses={apiLeaveStatuses?.items}
    leaveTypes={apiLeaveTypes?.items}
    leaveLimits={apiLeaveLimits?.items}
  />;
};

export function MyLeaveRequests() {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingAuth}
    >
      <DataContent />
    </MsalAuthenticationTemplate>
  );
}
