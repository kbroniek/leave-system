import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { LoadingAuth } from "../../components/Loading";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import { loginRequest } from "../../authConfig";
import { useState, useEffect } from "react";
import { ShowHrPanel } from "./ShowHrPanel";
import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { LeaveLimitDto, LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import { useSearchParams } from "react-router-dom";
import { DateTime } from "luxon";
import { useApiQuery } from "../../hooks/useApiQuery";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { EmployeesDto } from "../dtos/EmployeesDto";
import Paper from "@mui/material/Paper";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import Grid from "@mui/material/Grid";
import { Trans } from "react-i18next";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";

const DataContent = (): React.ReactElement => {
  const { inProgress, instance } = useMsal();
  const notifications = useNotifications();
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const queryYear = Number(searchParams.get("year"));
  const [currentYear, setCurrentYear] = useState<number>(
    !queryYear ? DateTime.local().year : queryYear
  );

  const now = DateTime.fromObject({ year: currentYear });
  const dateFromFormatted = now.startOf("year").toFormat("yyyy-MM-dd");
  const dateToFormatted = now.endOf("year").toFormat("yyyy-MM-dd");

  // Use TanStack Query for all API calls
  const { data: apiLeaveRequests } = useApiQuery<LeaveRequestsResponseDto>(
    ["leaveRequests", "hr", currentYear],
    `/leaverequests?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`,
    { enabled: inProgress === InteractionStatus.None }
  );

  // Use infinite query for leave limits with continuation tokens
  const {
    data: leaveLimitsData,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useInfiniteQuery<LeaveLimitsDto>({
    queryKey: ["leaveLimits", "hr", currentYear],
    queryFn: async ({ pageParam }) => {
      const account = instance.getActiveAccount();
      const url = pageParam
        ? `/leavelimits?year=${currentYear}&pageSize=1000&continuationToken=${encodeURIComponent(
            pageParam as string
          )}`
        : `/leavelimits?year=${currentYear}&pageSize=1000`;
      try {
        return await callApiGet<LeaveLimitsDto>(
          url,
          notifications.show,
          undefined,
          account,
          t
        );
      } catch (error) {
        await ifErrorAcquireTokenRedirect(error, instance);
        throw error;
      }
    },
    getNextPageParam: (lastPage) => lastPage.continuationToken ?? undefined,
    initialPageParam: undefined,
    enabled: inProgress === InteractionStatus.None,
  });

  // Fetch all pages automatically when there are more pages
  useEffect(() => {
    if (hasNextPage && !isFetchingNextPage) {
      void fetchNextPage();
    }
  }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

  // Flatten all pages into a single array
  const accumulatedLimits: LeaveLimitDto[] =
    leaveLimitsData?.pages.flatMap((page) => page.items) ?? [];

  const { data: apiHolidays } = useApiQuery<HolidaysDto>(
    ["holidays", currentYear],
    `/settings/holidays?dateFrom=${dateFromFormatted}&dateTo=${dateToFormatted}`,
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiLeaveTypes } = useApiQuery<LeaveTypesDto>(
    ["leaveTypes"],
    "/leavetypes",
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiEmployees } = useApiQuery<EmployeesDto>(
    ["employees"],
    "/employees",
    { enabled: inProgress === InteractionStatus.None }
  );
  return (
    <>
      <Paper elevation={3} sx={{ margin: "3px 0", width: "100%", padding: 1 }}>
        <Grid container spacing={0} sx={{ justifyContent: "center" }}>
          <Typography sx={{ alignContent: "center", padding: 2 }}>
            <Trans>Year</Trans>
          </Typography>
          <TextField
            type="number"
            value={currentYear}
            onChange={(v) => {
              const value = Number(v.target.value);
              if (value !== currentYear) {
                setCurrentYear(value);
                setSearchParams({ year: v.target.value });
              }
            }}
          />
        </Grid>
      </Paper>
      <Paper elevation={3} sx={{ margin: "3px 0", width: "100%" }}>
        <ShowHrPanel
          leaveRequests={apiLeaveRequests?.items}
          leaveTypes={apiLeaveTypes?.items.filter((x) => x.state === "Active")}
          leaveLimits={accumulatedLimits.filter((x) => x.state === "Active")}
          employees={apiEmployees?.items}
          holidays={apiHolidays?.items}
        />
      </Paper>
    </>
  );
};
export const HrPanel = (): React.ReactElement => (
  <MsalAuthenticationTemplate
    interactionType={InteractionType.Redirect}
    authenticationRequest={loginRequest}
    errorComponent={ErrorComponent}
    loadingComponent={LoadingAuth}
  >
    <Authorized
      roles={["GlobalAdmin", "HumanResource"]}
      authorized={<DataContent />}
      unauthorized={<Forbidden />}
    />
  </MsalAuthenticationTemplate>
);
