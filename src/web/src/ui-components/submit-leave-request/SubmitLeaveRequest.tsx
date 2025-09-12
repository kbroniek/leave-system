import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { useApiCall } from "../../hooks/useApiCall";
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
import { v4 as uuidv4 } from "uuid";
import { Authorized } from "../../components/Authorized";
import { isInRole } from "../../utils/roleUtils";
import { Forbidden } from "../../components/Forbidden";
import { useNotifications } from "@toolpad/core/useNotifications";
import { useTranslation } from "react-i18next";

const DataContent = () => {
  const { t } = useTranslation();
  const { instance, inProgress } = useMsal();
  const { callApi, callApiGet } = useApiCall();
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
  const [currentYear, setCurrentYear] = useState<string>(
    DateTime.local().toFormat("yyyy"),
  );
  const [currentUserId, setCurrentUserId] = useState<string | undefined>();
  const navigate = useNavigate();
  const notifications = useNotifications();

  const callHolidays = useCallback(
    async (dateFromFormatted: string, dateToFormatted: string) =>
      callApiGet<HolidaysDto>(
        `/settings/holidays?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`,
        notifications.show,
      )
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance)),
    [callApiGet, notifications.show, instance],
  );

  const callLeaveRequests = useCallback(
    (dateFromFormatted: string, dateToFormatted: string, userId: string) =>
      callApiGet<LeaveRequestsResponseDto>(
        `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}${userId ? `&AssignedToUserIds=${userId}` : ""}`,
        notifications.show,
      )
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance)),
    [callApiGet, notifications.show, instance],
  );

  const callLeaveLimits = useCallback(
    (userId: string, year: string) =>
      callApiGet<LeaveLimitsDto>(
        `/leavelimits?year=${year}&userIds=${userId}`,
        notifications.show,
      )
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance)),
    [callApiGet, notifications.show, instance],
  );

  const callMyLimits = useCallback(
    (year: string) => {
      callApiGet<LeaveLimitsDto>(
        `/leavelimits/user?year=${year}`,
        notifications.show,
      )
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    },
    [callApiGet, notifications.show, instance],
  );

  const isDecisionMaker = useCallback(
    () => isInRole(instance, ["DecisionMaker", "GlobalAdmin"]),
    [instance],
  );

  useEffect(() => {
    if (inProgress === InteractionStatus.None) {
      const claims = instance.getActiveAccount()?.idTokenClaims;
      const currentDate = DateTime.local();
      const dateFromFormatted = currentDate
        .startOf("year")
        .toFormat("yyyy-MM-dd");
      const dateToFormatted = currentDate.endOf("year").toFormat("yyyy-MM-dd");

      callApiGet<LeaveTypesDto>("/leavetypes", notifications.show)
        .then((response) => setApiLeaveTypes(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));

      callHolidays(dateFromFormatted, dateToFormatted);

      if (isDecisionMaker()) {
        callApiGet<EmployeesDto>("/employees", notifications.show)
          .then((response) => setApiEmployees(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      } else {
        callMyLimits(currentYear);
        if (claims?.sub) {
          setApiEmployees({
            items: [
              {
                id: claims?.sub,
                name: claims?.name ?? "",
              },
            ],
          });
        }
      }
    }
  }, [
    inProgress,
    callApiGet,
    callHolidays,
    callMyLimits,
    isDecisionMaker,
    currentYear,
    notifications.show,
    instance,
  ]);

  const onSubmit = async (model: LeaveRequestFormModel) => {
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
        },
      );
      return;
    }
    if (!model.workingDays) {
      notifications.show(
        t(
          "This leave type can not set in free day.Form is invalid. Can't read working days.",
        ),
        {
          severity: "warning",
          autoHideDuration: 3000,
        },
      );
      return;
    }
    if (!model.workingHours) {
      notifications.show(
        t(
          "Form is invalid. Can't read working hours. Check if you have added limits.",
        ),
        {
          severity: "warning",
          autoHideDuration: 3000,
        },
      );
      return;
    }
    if (!apiLeaveTypes) {
      notifications.show(
        t(
          "Form is invalid. Can't read leave types. Contact with administrator.",
        ),
        {
          severity: "warning",
          autoHideDuration: 3000,
        },
      );
      // TODO: Email to the administrator and admin name
      return;
    }
    const calculateDuration = (): Duration => {
      const leaveType = apiLeaveTypes.items.find(
        (x) => x.id === model.leaveType,
      );
      const duration =
        (leaveType?.properties?.includeFreeDays ?? true)
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
    const response = await callApi(url, "POST", body, notifications.show);
    if (response.status === 201) {
      notifications.show(t("Leave Request is added successfully"), {
        severity: "success",
        autoHideDuration: 3000,
      });
      navigate("/");
      return response.status;
    }
  };

  useEffect(() => {
    if (!currentUserId) {
      return;
    }
    setApiLeaveRequests(undefined);
    setApiLeaveLimits(undefined);
    const currentDate = DateTime.fromObject({ year: Number(currentYear) });
    const dateFromFormatted = currentDate
      .startOf("year")
      .toFormat("yyyy-MM-dd");
    const dateToFormatted = currentDate.endOf("year").toFormat("yyyy-MM-dd");
    Promise.all([
      callLeaveRequests(dateFromFormatted, dateToFormatted, currentUserId),
      isDecisionMaker()
        ? callLeaveLimits(currentUserId, currentYear)
        : callMyLimits(currentYear),
    ]);
  }, [
    currentUserId,
    currentYear,
    callLeaveRequests,
    callLeaveLimits,
    callMyLimits,
    isDecisionMaker,
  ]);

  return (
    <Authorized
      roles={["DecisionMaker", "GlobalAdmin", "Employee"]}
      authorized={
        <SubmitLeaveRequestForm
          leaveRequests={apiLeaveRequests?.items}
          holidays={apiHolidays}
          leaveTypes={apiLeaveTypes?.items.filter((x) => x.state === "Active")}
          leaveLimits={apiLeaveLimits?.items.filter(
            (x) => x.state === "Active",
          )}
          employees={apiEmployees?.items}
          onSubmit={onSubmit}
          onYearChanged={setCurrentYear}
          onUserIdChanged={setCurrentUserId}
        />
      }
      unauthorized={<Forbidden />}
    />
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
