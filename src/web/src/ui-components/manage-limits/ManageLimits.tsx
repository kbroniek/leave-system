import { useEffect, useState } from "react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { loginRequest } from "../../authConfig";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Loading, LoadingAuth } from "../../components/Loading";
import { LeaveLimitCell, ManageLimitsTable } from "./ManageLimitsTable";
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

  const handleLimitChange = async (cell: LeaveLimitCell): Promise<void> => {
    const workingHours = cell.workingHours ?? 8;
    const dto: LeaveLimitDto = {
      id: cell.id,
      assignedToUserId: cell.assignedToUserId,
      description: cell.description,
      leaveTypeId: cell.leaveTypeId,
      limit: cell.limit ? Duration.fromObject({hours: cell.limit * workingHours}).toISO() : null,
      overdueLimit: cell.overdueLimit ? Duration.fromObject({hours: cell.overdueLimit * workingHours}).toISO() : null,
      workingHours: Duration.fromObject({ hours: workingHours }).toISO(),
      validSince: cell.validSince ? cell.validSince.toISOString().split('T')[0] : null,
      validUntil: cell.validUntil ? cell.validUntil.toISOString().split('T')[0] : null,
      state: "Active",
    }
    console.log(dto);
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



// class DurationConverter {
//   public static convert(value: string, workingHours: string): DurationMaybeValid {
//     if(!value || value.trim() === "") {
//       return Duration.invalid("Value is empty");
//     }
//     const workingHoursBuffer = Duration.fromISO(workingHours);
//     if (!workingHoursBuffer.isValid) {
//       Duration.invalid(`Invalid workingHours: ${workingHours}`);
//     }
//     const regexp = /((\d+)[d])/g;
//     const match = regexp.exec(value);

//     if(!match) {
//       return Duration.invalid("Value is invalid. You can provide only days.");
//     }
    
//     const days = Number(match[2]);
//     return Duration.fromObject({hours: days / workingHoursBuffer.as("hours")});
//   }
// }