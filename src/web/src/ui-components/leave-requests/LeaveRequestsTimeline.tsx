import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { loginRequest } from "../../authConfig";

// Sample app imports
import { Loading, LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
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
  const [apiLeaveRequests, setApiLeaveRequests] =
    useState<LeaveRequestsResponseDto | null>(null);
  const [apiHolidays, setApiHolidays] = useState<HolidaysDto | null>(null);
  const [apiLeaveStatuses, setApiLeaveStatuses] =
    useState<LeaveStatusesDto | null>(null);
  const [apiLeaveTypes, setApiLeaveTypes] = useState<LeaveTypesDto | null>(
    null,
  );
  const [apiEmployees, setApiEmployees] = useState<EmployeesDto | null>(null);
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
