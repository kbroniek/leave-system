import { useCallback, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading, LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { useApiQuery } from "../../hooks/useApiQuery";
import { useApiMutation } from "../../hooks/useApiMutation";
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
  const now = DateTime.now().startOf("day");
  const [dateFrom, setDateFrom] = useState<DateTime>(now.minus({ days: 14 }));
  const [dateTo, setDateTo] = useState<DateTime>(now.plus({ days: 14 }));
  const [leaveTypes, setLeaveTypes] = useState<string[] | undefined>([]);
  const [employeesSearch, setEmployeesSearch] = useState<string[]>([]);
  const [statusesSearch, setStatusesSearch] = useState<string[]>(
    leaveRequestsStatuses.slice(0, 3),
  );
  const [selectedUserId, setSelectedUserId] = useState<string | undefined>();
  const [selectedYear, setSelectedYear] = useState<string>(DateTime.now().toFormat("yyyy"));
  const notifications = useNotifications();
  const { t } = useTranslation();

  const isDecisionMaker = useCallback(
    () => isInRole(instance, ["DecisionMaker", "GlobalAdmin"]),
    [instance],
  );

  // Build query URL for leave requests
  const leaveRequestsUrl = `/leaverequests?dateFrom=${dateFrom.toFormat("yyyy-MM-dd")}&dateTo=${dateTo.toFormat("yyyy-MM-dd")}${employeesSearch.map((x) => `&assignedToUserIds=${x}`).join("")}${leaveTypes?.map((x) => `&leaveTypeIds=${x}`).join("")}${statusesSearch.map((x) => `&statuses=${x}`).join("")}`;

  // Use TanStack Query for all API calls
  const { data: apiLeaveRequests } = useApiQuery<LeaveRequestsResponseDto>(
    ["leaveRequests", "timeline", dateFrom.toFormat("yyyy-MM-dd"), dateTo.toFormat("yyyy-MM-dd"), employeesSearch.join(","), leaveTypes?.join(","), statusesSearch.join(",")],
    leaveRequestsUrl,
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiHolidays } = useApiQuery<HolidaysDto>(
    ["holidays", "timeline", dateFrom.toFormat("yyyy-MM-dd"), dateTo.toFormat("yyyy-MM-dd")],
    `/settings/holidays?dateFrom=${dateFrom.toFormat("yyyy-MM-dd")}&dateTo=${dateTo.toFormat("yyyy-MM-dd")}`,
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiLeaveStatuses } = useApiQuery<LeaveStatusesDto>(
    ["leaveStatuses"],
    "/settings/leavestatus",
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiLeaveTypes } = useApiQuery<LeaveTypesDto>(
    ["leaveTypes"],
    "/leavetypes",
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiEmployees } = useApiQuery<EmployeesDto>(
    ["employees", "timeline"],
    "/employees",
    { enabled: inProgress === InteractionStatus.None && isDecisionMaker() }
  );

  const { data: apiLeaveLimits } = useApiQuery<LeaveLimitsDto>(
    ["leaveLimits", "timeline", selectedUserId, selectedYear],
    selectedUserId && isDecisionMaker()
      ? `/leavelimits?year=${selectedYear}&userIds=${selectedUserId}`
      : `/leavelimits/user?year=${selectedYear}`,
    { enabled: inProgress === InteractionStatus.None && !!selectedUserId }
  );

  // Set employees from token if not a decision maker
  const employeesFromToken = useCallback(() => {
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
      }
    },
    invalidateQueries: [
      ["leaveRequests"],
      ["leaveLimits"],
    ],
  });

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
    submitLeaveRequestMutation.mutate({ url, method: "POST", body });
  };

  const onYearChanged = () => {
    // Handle year change if needed
  };

  const onUserIdChanged = (userId: string) => {
    setSelectedUserId(userId);
    setSelectedYear(DateTime.now().toFormat("yyyy"));
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
    setDateFrom(model.dateFrom);
    setDateTo(model.dateTo);
    setLeaveTypes(model.leaveTypes);
    setEmployeesSearch(model.employees);
    setStatusesSearch(model.statuses);
  };

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
            employees={finalEmployees?.items ?? []}
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
            employees={finalEmployees?.items ?? []}
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
