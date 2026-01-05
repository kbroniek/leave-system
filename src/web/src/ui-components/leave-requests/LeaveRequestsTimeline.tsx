import { useCallback, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading, LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { useApiQuery } from "../../hooks/useApiQuery";
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
import { DateTime } from "luxon";
import { leaveRequestsStatuses } from "../utils/Status";
import { useTranslation } from "react-i18next";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const now = DateTime.now().startOf("day");
  const [dateFrom, setDateFrom] = useState<DateTime>(now.minus({ days: 14 }));
  const [dateTo, setDateTo] = useState<DateTime>(now.plus({ days: 14 }));
  const [leaveTypes, setLeaveTypes] = useState<string[] | undefined>([]);
  const [employeesSearch, setEmployeesSearch] = useState<string[]>([]);
  const [statusesSearch, setStatusesSearch] = useState<string[]>(
    leaveRequestsStatuses.slice(0, 3)
  );
  const notifications = useNotifications();
  const { t } = useTranslation();

  const isDecisionMaker = useCallback(
    () => isInRole(instance, ["DecisionMaker", "GlobalAdmin"]),
    [instance]
  );

  // Build query URL for leave requests
  const leaveRequestsUrl = `/leaverequests?dateFrom=${dateFrom.toFormat(
    "yyyy-MM-dd"
  )}&dateTo=${dateTo.toFormat("yyyy-MM-dd")}${employeesSearch
    .map((x) => `&assignedToUserIds=${x}`)
    .join("")}${leaveTypes
    ?.map((x) => `&leaveTypeIds=${x}`)
    .join("")}${statusesSearch.map((x) => `&statuses=${x}`).join("")}`;

  // Use TanStack Query for all API calls
  const { data: apiLeaveRequests } = useApiQuery<LeaveRequestsResponseDto>(
    [
      "leaveRequests",
      "timeline",
      dateFrom.toFormat("yyyy-MM-dd"),
      dateTo.toFormat("yyyy-MM-dd"),
      employeesSearch.join(","),
      leaveTypes?.join(","),
      statusesSearch.join(","),
    ],
    leaveRequestsUrl,
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiHolidays } = useApiQuery<HolidaysDto>(
    [
      "holidays",
      "timeline",
      dateFrom.toFormat("yyyy-MM-dd"),
      dateTo.toFormat("yyyy-MM-dd"),
    ],
    `/settings/holidays?dateFrom=${dateFrom.toFormat(
      "yyyy-MM-dd"
    )}&dateTo=${dateTo.toFormat("yyyy-MM-dd")}`,
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

    const employeesToRender =
      employeesSearch.length && employeesSearch.filter((x) => !!x).length > 0
        ? apiEmployees?.items.filter((x) => employeesSearch.includes(x.id))
        : apiEmployees?.items;
    return {
      items: employeesToRender ?? [],
    } as EmployeesDto;
  }, [isDecisionMaker, inProgress, instance, apiEmployees, employeesSearch]);

  const finalEmployees = employeesFromToken();
  const onSubmit = (model: SearchLeaveRequestModel) => {
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
              (x) => x.state === "Active"
            )}
            leaveTypes={apiLeaveTypes.items}
            employees={finalEmployees?.items ?? []}
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
