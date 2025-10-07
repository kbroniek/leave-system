import { useCallback, useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading, LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { useApiCall } from "../../hooks/useApiCall";
import ShowLeaveRequestsTimeline from "./ShowLeaveRequestsTimeline";
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { useNotifications } from "@toolpad/core/useNotifications";
import { EmployeesDto } from "../dtos/EmployeesDto";
import {
  LeaveRequestsSearch,
  SearchLeaveRequestModel,
} from "./LeaveRequestsSearch";
import Box from "@mui/material/Box";
import Paper from "@mui/material/Paper";
import { isInRole } from "../../utils/roleUtils";
import { DateTime, Duration } from "luxon";
import { leaveRequestsStatuses } from "../utils/Status";
import { useTranslation } from "react-i18next";
import { LeaveRequestFormModel } from "../submit-leave-request/SubmitLeaveRequestForm";
import { v4 as uuidv4 } from "uuid";
import { LeaveLimitsDto } from "../dtos/LeaveLimitsDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const { callApi, callApiGet } = useApiCall();
  const [apiLeaveRequests, setApiLeaveRequests] =
    useState<LeaveRequestsResponseDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] =
    useState<LeaveStatusesDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(
    null,
  );
  const [apiEmployees, setApiEmployees] = useState<EmployeesDto | null>(null);
  const [apiLeaveLimits, setApiLeaveLimits] = useState<LeaveLimitsDto | null>(
    null,
  );
  const now = DateTime.now().startOf("day");
  const [dateFrom, setDateFrom] = useState<DateTime>(now.minus({ days: 14 }));
  const [dateTo, setDateTo] = useState<DateTime>(now.plus({ days: 14 }));
  const [leaveTypes, setLeaveTypes] = useState<string[] | undefined>([]);
  const [employeesSearch, setEmployeesSearch] = useState<string[]>([]);
  const [statusesSearch, setStatusesSearch] = useState<string[]>(
    leaveRequestsStatuses.slice(0, 3),
  );
  const [isCallApi, setIsCallApi] = useState(true);
  const notifications = useNotifications();
  const { t } = useTranslation();

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

  const onSubmitLeaveRequest = async (model: LeaveRequestFormModel) => {
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
      // Refresh the data
      setIsCallApi(true);
      return response.status;
    }
    return response.status;
  };

  const onYearChanged = (_year: string) => {
    // Handle year change if needed
  };

  const onUserIdChanged = (userId: string) => {
    const currentYear = DateTime.now().toFormat("yyyy");
    if (isDecisionMaker()) {
      callLeaveLimits(userId, currentYear);
    } else {
      callMyLimits(currentYear);
    }
  };

  const onSubmit = async (model: SearchLeaveRequestModel) => {
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
    setApiLeaveRequests(null);
    setDateFrom(model.dateFrom);
    setDateTo(model.dateTo);
    setLeaveTypes(model.leaveTypes);
    setEmployeesSearch(model.employees);
    setStatusesSearch(model.statuses);
    setIsCallApi(true);
  };

  useEffect(() => {
    if (isCallApi && inProgress === InteractionStatus.None) {
      setIsCallApi(false);
      callApiGet<LeaveRequestsResponseDto>(
        `/leaverequests?dateFrom=${dateFrom.toFormat("yyyy-MM-dd")}&dateTo=${dateTo.toFormat("yyyy-MM-dd")}${employeesSearch.map((x) => `&assignedToUserIds=${x}`).join("")}${leaveTypes?.map((x) => `&leaveTypeIds=${x}`).join("")}${statusesSearch.map((x) => `&statuses=${x}`).join("")}`,
        notifications.show,
      )
        .then((response) => setApiLeaveRequests(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      callApiGet<HolidaysDto>(
        `/settings/holidays?dateFrom=${dateFrom.toFormat("yyyy-MM-dd")}&dateTo=${dateTo.toFormat("yyyy-MM-dd")}`,
        notifications.show,
      )
        .then((response) => setApiHolidays(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      if (!apiLeaveStatuses) {
        callApiGet<LeaveStatusesDto>(
          "/settings/leavestatus",
          notifications.show,
        )
          .then((response) => setApiLeaveStatuses(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      }
      if (!apiLeaveTypes)
        callApiGet<LeaveTypesDto>("/leavetypes", notifications.show)
          .then((response) => setApiLeaveTypes(response))
          .catch((e) => ifErrorAcquireTokenRedirect(e, instance));

      if (!apiEmployees) {
        if (isInRole(instance, ["DecisionMaker", "GlobalAdmin"])) {
          callApiGet<EmployeesDto>("/employees", notifications.show)
            .then((response) => setApiEmployees(response))
            .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
        } else {
          const claims = instance.getActiveAccount()?.idTokenClaims;
          if (claims?.oid) {
            const name = claims?.name ?? "";
            const splitName = name.split(" ");
            const firstName = (claims?.given_name as string) ?? splitName[0];
            const lastName = (claims?.family_name as string) ?? splitName[1];
            setApiEmployees({
              items: [
                {
                  id: claims?.oid,
                  name: name,
                  firstName: firstName,
                  lastName: lastName,
                },
              ],
            });
          }
        }
      }
    }
  }, [inProgress, isCallApi]);

  const employeeToRender =
    employeesSearch.length && employeesSearch.filter((x) => !!x).length > 0
      ? apiEmployees?.items.filter((x) => employeesSearch.includes(x.id))
      : apiEmployees?.items;
  return (
    <>
      <Paper elevation={3} sx={{ margin: "3px 0", width: "100%" }}>
        <Box sx={{ padding: "20px 10px" }}>
          <LeaveRequestsSearch
            leaveTypes={apiLeaveTypes?.items}
            employees={apiEmployees?.items ?? []}
            onSubmit={onSubmit}
          />
        </Box>
      </Paper>
      <Paper elevation={3} sx={{ width: "100%" }}>
        {apiLeaveRequests &&
        apiHolidays &&
        apiLeaveStatuses &&
        apiLeaveTypes ? (
          <ShowLeaveRequestsTimeline
            leaveRequests={apiLeaveRequests}
            holidays={apiHolidays}
            leaveStatuses={apiLeaveStatuses.items.filter(
              (x) => x.state === "Active",
            )}
            leaveTypes={apiLeaveTypes.items}
            employees={employeeToRender ?? []}
            leaveLimits={apiLeaveLimits?.items}
            onSubmitLeaveRequest={onSubmitLeaveRequest}
            onYearChanged={onYearChanged}
            onUserIdChanged={onUserIdChanged}
          />
        ) : (
          <Box sx={{ justifyContent: "center", display: "flex" }}>
            <Loading />
          </Box>
        )}
      </Paper>
    </>
  );
};

export function LeaveRequestsTimeline() {
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
