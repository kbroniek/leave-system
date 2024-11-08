import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { LoadingAuth } from "../Loading";
import { ErrorComponent } from "../ErrorComponent";
import { callApi, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveRequestsResponseDto } from "../leave-requests/LeaveRequestsDto";
import { LeaveRequestFormModel, SubmitLeaveRequestForm } from "./SubmitLeaveRequestForm";
import { DateTime } from "luxon";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import { EmployeesDto } from "../dtos/EmployeesDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const [apiLeaveRequests, setApiLeaveRequests] = useState<
    LeaveRequestsResponseDto | undefined
  >();
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | undefined>();
  const [apiLeaveTypes, setApiLeaveTypes] = useState<
    LeaveTypesDto | undefined
  >();
  const [apiLeaveLimits, setApiLeaveLimits] = useState<
    LeaveLimitsDto | undefined
  >();
  const [apiEmployees, setApiEmployees] = useState<EmployeesDto | undefined>();

  useEffect(() => {
    if (!apiLeaveRequests && inProgress === InteractionStatus.None) {
      const userId = instance.getActiveAccount()?.idTokenClaims?.sub;
      const now = DateTime.local();
      const currentYear = now.toFormat("yyyy");
      const dateFromFormatted = now
        .startOf("year")
        .toFormat("yyyy-MM-dd");
      const dateToFormatted = now
        .endOf("year")
        .toFormat("yyyy-MM-dd");
      callApi<LeaveRequestsResponseDto>(
        `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}&AssignedToUserIds=${userId}`
      )
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<LeaveTypesDto>("/leavetypes")
        .then((response) => setApiLeaveTypes(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));

      callApi<HolidaysDto>(
        `/settings/holidays?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`
      )
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<LeaveLimitsDto>(`/leavelimits/user?year=${currentYear}`)
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApi<EmployeesDto>("/employees")
        .then((response) => setApiEmployees(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress, apiLeaveRequests, instance]);

  const onSubmit = (value: LeaveRequestFormModel) => {
    alert(JSON.stringify(value));
  };

  return (
    <>
      <SubmitLeaveRequestForm
        leaveRequests={apiLeaveRequests?.items}
        holidays={apiHolidays}
        leaveTypes={apiLeaveTypes?.items}
        leaveLimits={apiLeaveLimits?.items}
        employees={apiEmployees?.items}
        onSubmit={onSubmit}
      />
    </>
  );
};

export function SubmitLeaveRequest() {
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
