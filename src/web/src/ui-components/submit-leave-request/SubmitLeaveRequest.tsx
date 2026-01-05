import { useCallback, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { useApiQuery } from "../../hooks/useApiQuery";
import { useApiMutation } from "../../hooks/useApiMutation";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto";
import {
  LeaveRequestFormModel,
  SubmitLeaveRequestForm,
} from "./SubmitLeaveRequestForm";
import { DateTime, Duration } from "luxon";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import { EmployeesDto } from "../dtos/EmployeesDto";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { v4 as uuidv4 } from "uuid";
import { Authorized } from "../../components/Authorized";
import { isInRole } from "../../utils/roleUtils";
import { Forbidden } from "../../components/Forbidden";
import { useNotifications } from "@toolpad/core/useNotifications";
import { useTranslation } from "react-i18next";

interface DataContentProps {
  initialDate?: DateTime;
  initialEmployee?: EmployeeDto;
  onSuccess?: () => void;
}

const DataContent = ({
  initialDate,
  initialEmployee,
  onSuccess,
}: DataContentProps) => {
  const { t } = useTranslation();
  const { instance, inProgress } = useMsal();
  const [currentYear, setCurrentYear] = useState<string>(
    initialDate?.toFormat("yyyy") ?? DateTime.local().toFormat("yyyy")
  );
  const [currentUserId, setCurrentUserId] = useState<string | undefined>(
    initialEmployee?.id
  );
  const notifications = useNotifications();

  const isDecisionMaker = useCallback(
    () => isInRole(instance, ["DecisionMaker", "GlobalAdmin"]),
    [instance]
  );

  const currentDate = DateTime.fromFormat(currentYear, "yyyy");
  const dateFromFormatted = currentDate.startOf("year").toFormat("yyyy-MM-dd");
  const dateToFormatted = currentDate.endOf("year").toFormat("yyyy-MM-dd");

  // Use TanStack Query for all API calls
  const { data: apiLeaveTypes } = useApiQuery<LeaveTypesDto>(
    ["leaveTypes"],
    "/leavetypes",
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiHolidays } = useApiQuery<HolidaysDto>(
    ["holidays", "submit", currentYear],
    `/settings/holidays?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`,
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiEmployees } = useApiQuery<EmployeesDto>(
    ["employees"],
    "/employees",
    { enabled: inProgress === InteractionStatus.None && isDecisionMaker() }
  );

  const { data: apiLeaveLimits } = useApiQuery<LeaveLimitsDto>(
    ["leaveLimits", "user", currentYear],
    `/leavelimits/user?year=${currentYear}`,
    { enabled: inProgress === InteractionStatus.None && !isDecisionMaker() }
  );

  const { data: apiLeaveLimitsForUser } = useApiQuery<LeaveLimitsDto>(
    ["leaveLimits", currentUserId, currentYear],
    `/leavelimits?year=${currentYear}&userIds=${currentUserId}`,
    {
      enabled:
        inProgress === InteractionStatus.None &&
        !!currentUserId &&
        isDecisionMaker(),
    }
  );

  const { data: apiLeaveRequests } = useApiQuery<LeaveRequestsResponseDto>(
    ["leaveRequests", "submit", currentUserId, currentYear],
    `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}${
      currentUserId ? `&AssignedToUserIds=${currentUserId}` : ""
    }`,
    { enabled: inProgress === InteractionStatus.None && !!currentUserId }
  );

  // Get employees from token if not a decision maker
  const employeesFromToken = useCallback((): EmployeesDto | undefined => {
    if (!isDecisionMaker() && inProgress === InteractionStatus.None) {
      const claims = instance.getActiveAccount()?.idTokenClaims;
      if (claims?.oid) {
        const name = claims?.name ?? "";
        const splitName = name.split(" ");
        const firstName = (claims?.given_name as string) ?? splitName[0];
        const lastName = (claims?.family_name as string) ?? splitName[1];
        return {
          items: [
            {
              id: claims?.oid,
              name: name,
              firstName: firstName,
              lastName: lastName,
            },
          ],
        } as EmployeesDto;
      }
    }
    return apiEmployees;
  }, [isDecisionMaker, inProgress, instance, apiEmployees]);

  const finalEmployees = employeesFromToken();

  // Create mutation for submitting leave request
  const submitLeaveRequestMutation = useApiMutation({
    onSuccess: (response) => {
      if (response.status === 201) {
        notifications.show(t("Leave Request is added successfully"), {
          severity: "success",
          autoHideDuration: 3000,
        });
        if (onSuccess) {
          onSuccess();
        }
      }
    },
    invalidateQueries: [["leaveRequests"], ["leaveLimits"]],
  });

  const isSubmitting = submitLeaveRequestMutation.isPending;

  const onSubmit = (model: LeaveRequestFormModel) => {
    if (!model.dateFrom?.isValid) {
      notifications.show(t("Date from is invalid. Choose correct date."), {
        severity: "warning",
        autoHideDuration: 3000,
      });
      return;
    }
    if (!model.dateTo?.isValid) {
      notifications.show(t("Date to is invalid. Choose correct date."), {
        severity: "warning",
        autoHideDuration: 3000,
      });
      return;
    }
    if (!model.leaveType) {
      notifications.show(t("Leave type is invalid. Choose correct item."), {
        severity: "warning",
        autoHideDuration: 3000,
      });
      return;
    }
    if (!model.allDays) {
      notifications.show(
        t("Form is invalid. Can't read all days. Contact with administrator."),
        {
          severity: "warning",
          autoHideDuration: 3000,
        }
      );
      return;
    }
    if (!model.workingDays) {
      notifications.show(
        t(
          "This leave type can not set in free day.Form is invalid. Can't read working days."
        ),
        {
          severity: "warning",
          autoHideDuration: 3000,
        }
      );
      return;
    }
    if (!model.workingHours) {
      notifications.show(
        t(
          "Form is invalid. Can't read working hours. Check if you have added limits."
        ),
        {
          severity: "warning",
          autoHideDuration: 3000,
        }
      );
      return;
    }
    if (!apiLeaveTypes) {
      notifications.show(
        t(
          "Form is invalid. Can't read leave types. Contact with administrator."
        ),
        {
          severity: "warning",
          autoHideDuration: 3000,
        }
      );
      // TODO: Email to the administrator and admin name
      return;
    }
    const calculateDuration = (): Duration => {
      const leaveType = apiLeaveTypes.items.find(
        (x) => x.id === model.leaveType
      );
      const duration =
        leaveType?.properties?.includeFreeDays ?? true
          ? model.workingHours!.as("milliseconds") * model.allDays!
          : model.workingHours!.as("milliseconds") * model.workingDays!;

      return Duration.fromObject({
        days: 0,
        hours: 0,
        seconds: 0,
        milliseconds: duration,
      }).normalize();
    };
    const isNotOnBehalf =
      !model.onBehalf ||
      model.onBehalf === instance.getActiveAccount()?.localAccountId;
    const url = isNotOnBehalf ? "/leaverequests" : "/leaverequests/onbehalf";
    const body = {
      leaveRequestId: uuidv4(),
      dateFrom: model.dateFrom.toFormat("yyyy-MM-dd"),
      dateTo: model.dateTo.toFormat("yyyy-MM-dd"),
      duration: calculateDuration().toISO(),
      workingHours: model.workingHours.toISO(),
      leaveTypeId: model.leaveType,
      remark: model.remarks,
      assignedToId: isNotOnBehalf ? undefined : model.onBehalf,
    };
    void submitLeaveRequestMutation.mutate({ url, method: "POST", body });
  };

  const finalLeaveLimits =
    isDecisionMaker() && currentUserId ? apiLeaveLimitsForUser : apiLeaveLimits;

  return (
    <Authorized
      roles={["DecisionMaker", "GlobalAdmin", "Employee"]}
      authorized={
        <SubmitLeaveRequestForm
          leaveRequests={apiLeaveRequests?.items}
          holidays={apiHolidays}
          leaveTypes={apiLeaveTypes?.items.filter((x) => x.state === "Active")}
          leaveLimits={finalLeaveLimits?.items.filter(
            (x) => x.state === "Active"
          )}
          employees={finalEmployees?.items}
          onSubmit={onSubmit}
          onYearChanged={setCurrentYear}
          onUserIdChanged={setCurrentUserId}
          initialValues={{
            dateFrom: initialDate,
            dateTo: initialDate,
            onBehalf: initialEmployee?.id,
          }}
          initialEmployee={initialEmployee}
          isSubmitting={isSubmitting}
        />
      }
      unauthorized={<Forbidden />}
    />
  );
};

interface SubmitLeaveRequestProps {
  initialDate?: DateTime;
  initialEmployee?: EmployeeDto;
  onSuccess?: () => void;
}

export function SubmitLeaveRequest(
  {
    initialDate,
    initialEmployee,
    onSuccess,
  }: SubmitLeaveRequestProps = {} as SubmitLeaveRequestProps
) {
  return (
    <MsalAuthenticationTemplate
      interactionType={InteractionType.Redirect}
      authenticationRequest={loginRequest}
      errorComponent={ErrorComponent}
      loadingComponent={LoadingAuth}
    >
      <DataContent
        initialDate={initialDate}
        initialEmployee={initialEmployee}
        onSuccess={onSuccess}
      />
    </MsalAuthenticationTemplate>
  );
}
