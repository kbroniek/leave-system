import { useEffect, useState } from "react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { loginRequest } from "../../authConfig";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Loading, LoadingAuth } from "../../components/Loading";
import { LeaveLimitCell as LeaveLimitItem, ManageLimitsTable } from "./ManageLimitsTable";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApi, callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import { EmployeesDto } from "../dtos/EmployeesDto";
import { LeaveLimitDto, LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { DateTime, Duration } from "luxon";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const notifications = useNotifications();
  const [apiEmployees, setApiEmployees] = useState<EmployeesDto | undefined>();
  const [apiLeaveTypes, setApiLeaveTypes] = useState<
    LeaveTypesDto | undefined
  >();
  const [apiLeaveLimits, setApiLeaveLimits] = useState<
    LeaveLimitsDto | undefined
  >();
  const [currentYear, setCurrentYear] = useState<number>(DateTime.local().year);

  const handleLimitChange = async (item: LeaveLimitItem): Promise<void> => {
    const workingHours = item.workingHours ?? 8;
    const dto: LeaveLimitDto = {
      id: item.id,
      assignedToUserId: item.assignedToUserId,
      description: item.description,
      leaveTypeId: item.leaveTypeId,
      limit: item.limit ? Duration.fromObject({hours: item.limit * workingHours}).toISO() : null,
      overdueLimit: item.overdueLimit ? Duration.fromObject({hours: item.overdueLimit * workingHours}).toISO() : null,
      workingHours: Duration.fromObject({ hours: workingHours }).toISO(),
      validSince: item.validSince ? DateTime.fromJSDate(item.validSince).toFormat("yyyy-MM-dd") : null,
      validUntil: item.validUntil ? DateTime.fromJSDate(item.validUntil).toFormat("yyyy-MM-dd") : null,
      state: item.state,
    }
    const response = await callApi(`/leavelimits/${dto.id}`, "PUT", dto, notifications.show);
    if (response.status === 200) {
      notifications.show("Limit is updated successfully", {
        severity: "success",
        autoHideDuration: 3000,
      });
    }
  };

  useEffect(() => {
    if (inProgress === InteractionStatus.None) {
      callApiGet<EmployeesDto>("/employees", notifications.show)
        .then((response) => setApiEmployees(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<LeaveTypesDto>("/leavetypes", notifications.show)
        .then((response) => setApiLeaveTypes(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    callApiGet<LeaveLimitsDto>(
      `/leavelimits?year=${currentYear}`,
      notifications.show
    )
      .then((response) => setApiLeaveLimits(response))
      .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress]);
  return apiEmployees && apiLeaveTypes && apiLeaveLimits ? (
    <Authorized
      roles={["LeaveLimitAdmin", "GlobalAdmin"]}
      authorized={
        <ManageLimitsTable
          employees={apiEmployees.items}
          leaveTypes={apiLeaveTypes.items.filter(x => x.state === "Active")}
          limits={apiLeaveLimits?.items.filter(x => x.state === "Active")}
          limitOnChange={handleLimitChange}
        />
      }
      unauthorized={<Forbidden />}
    />
  ) : (
    <Loading />
  );
};

export const ManageLimits = () => (
  <MsalAuthenticationTemplate
    interactionType={InteractionType.Redirect}
    authenticationRequest={loginRequest}
    errorComponent={ErrorComponent}
    loadingComponent={LoadingAuth}
  >
    <DataContent />
  </MsalAuthenticationTemplate>
);