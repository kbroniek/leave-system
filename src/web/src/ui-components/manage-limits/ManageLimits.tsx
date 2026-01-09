import { useState, useEffect, useCallback } from "react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { loginRequest } from "../../authConfig";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Loading, LoadingAuth } from "../../components/Loading";
import {
  LeaveLimitCell as LeaveLimitItem,
  ManageLimitsTable,
} from "./ManageLimitsTable";
import { GenerateNewYearLimitsModal } from "./GenerateNewYearLimitsModal";
import { useNotifications } from "@toolpad/core/useNotifications";
import { useApiQuery } from "../../hooks/useApiQuery";
import { useApiMutation } from "../../hooks/useApiMutation";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import { EmployeesDto } from "../dtos/EmployeesDto";
import { LeaveLimitDto, LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { DateTime, Duration } from "luxon";
import Paper from "@mui/material/Paper";
import Grid from "@mui/material/Grid";
import Typography from "@mui/material/Typography";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import AddIcon from "@mui/icons-material/Add";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";

const DataContent = () => {
  const { inProgress, instance } = useMsal();
  const notifications = useNotifications();
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [currentYear, setCurrentYear] = useState<number>(DateTime.local().year);
  const [isGenerateModalOpen, setIsGenerateModalOpen] =
    useState<boolean>(false);
  const [accumulatedLimits, setAccumulatedLimits] = useState<LeaveLimitDto[]>(
    []
  );
  const [continuationToken, setContinuationToken] = useState<string | null>(
    null
  );
  const [isLoadingMore, setIsLoadingMore] = useState<boolean>(false);

  // Use TanStack Query for all API calls
  const { data: apiEmployees } = useApiQuery<EmployeesDto>(
    ["employees"],
    "/employees",
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiLeaveTypes } = useApiQuery<LeaveTypesDto>(
    ["leaveTypes"],
    "/leavetypes",
    { enabled: inProgress === InteractionStatus.None }
  );

  const { data: apiLeaveLimits } = useApiQuery<LeaveLimitsDto>(
    ["leaveLimits", "manage", currentYear],
    `/leavelimits?year=${currentYear}`,
    { enabled: inProgress === InteractionStatus.None }
  );

  // Reset accumulated limits when year changes or initial data loads
  useEffect(() => {
    if (apiLeaveLimits) {
      setAccumulatedLimits(apiLeaveLimits.items);
      const token =
        apiLeaveLimits.continuationToken !== undefined
          ? apiLeaveLimits.continuationToken
          : null;
      setContinuationToken(token);
    }
  }, [apiLeaveLimits, currentYear]);

  // Create mutation for updating/creating limits
  const updateLimitMutation = useApiMutation({
    onSuccess: (_, variables: { method: string }) => {
      const isCreate = variables.method === "POST";
      notifications.show(
        isCreate
          ? t("Limit is created successfully")
          : t("Limit is updated successfully"),
        {
          severity: "success",
          autoHideDuration: 3000,
        }
      );
    },
    invalidateQueries: [["leaveLimits", "manage", currentYear]],
  });

  const handleLimitChange = async (item: LeaveLimitItem): Promise<boolean> => {
    const workingHours = item.workingHours ?? 8;
    const dto: LeaveLimitDto = {
      id: item.id,
      assignedToUserId: item.assignedToUserId,
      description: item.description,
      leaveTypeId: item.leaveTypeId,
      limit: item.limit
        ? Duration.fromObject({ hours: item.limit * workingHours }).toISO()
        : null,
      overdueLimit: item.overdueLimit
        ? Duration.fromObject({
            hours: item.overdueLimit * workingHours,
          }).toISO()
        : null,
      workingHours: Duration.fromObject({ hours: workingHours }).toISO(),
      validSince: item.validSince
        ? DateTime.fromJSDate(item.validSince).toFormat("yyyy-MM-dd")
        : null,
      validUntil: item.validUntil
        ? DateTime.fromJSDate(item.validUntil).toFormat("yyyy-MM-dd")
        : null,
      state: item.state,
    };

    // Check if this is a new limit (doesn't exist in accumulated limits)
    const isNewLimit = !accumulatedLimits.some((limit) => limit.id === dto.id);

    updateLimitMutation.mutate({
      url: isNewLimit ? `/leavelimits` : `/leavelimits/${dto.id}`,
      method: isNewLimit ? "POST" : "PUT",
      body: dto,
    });
    return Promise.resolve(true);
  };

  const handleLoadMore = useCallback(async () => {
    if (!continuationToken || isLoadingMore) return;

    setIsLoadingMore(true);
    try {
      const account = instance.getActiveAccount();
      const url = `/leavelimits?year=${currentYear}&continuationToken=${encodeURIComponent(
        continuationToken
      )}`;
      const nextPage = await callApiGet<LeaveLimitsDto>(
        url,
        notifications.show,
        undefined,
        account,
        t
      );

      if (nextPage) {
        setAccumulatedLimits((prev) => [...prev, ...nextPage.items]);
        const token =
          nextPage.continuationToken !== undefined
            ? nextPage.continuationToken
            : null;
        setContinuationToken(token);
      }
    } catch (error) {
      await ifErrorAcquireTokenRedirect(error, instance);
      notifications.show(t("Failed to load more limits"), {
        severity: "error",
      });
    } finally {
      setIsLoadingMore(false);
    }
  }, [
    continuationToken,
    currentYear,
    isLoadingMore,
    instance,
    notifications,
    t,
  ]);

  const handleLimitsGenerated = () => {
    // Query will automatically refetch due to invalidation
    void queryClient.invalidateQueries({
      queryKey: ["leaveLimits", "manage", currentYear],
    });
  };
  return (
    <>
      <Paper elevation={3} sx={{ margin: "3px 0", width: "100%", padding: 1 }}>
        <Grid
          container
          spacing={2}
          sx={{ justifyContent: "center", alignItems: "center" }}
        >
          <Typography sx={{ alignContent: "center", padding: 2 }}>
            Year
          </Typography>
          <TextField
            type="number"
            value={currentYear}
            onChange={(v) => {
              const value = Number(v.target.value);
              if (value !== currentYear) {
                setCurrentYear(value);
              }
            }}
          />
          <Button
            variant="outlined"
            startIcon={<AddIcon />}
            onClick={() => setIsGenerateModalOpen(true)}
            sx={{ marginLeft: 2 }}
          >
            {t("Generate New Year Limits")}
          </Button>
        </Grid>
      </Paper>
      <Paper elevation={3} sx={{ margin: "3px 0", width: "100%" }}>
        <Authorized
          roles={["LeaveLimitAdmin", "GlobalAdmin"]}
          authorized={
            apiEmployees && apiLeaveTypes && apiLeaveLimits ? (
              <>
                <ManageLimitsTable
                  employees={apiEmployees.items}
                  leaveTypes={apiLeaveTypes.items.filter(
                    (x) => x.state === "Active"
                  )}
                  limits={accumulatedLimits.filter((x) => x.state === "Active")}
                  limitOnChange={handleLimitChange}
                  onLoadMore={
                    continuationToken ? () => void handleLoadMore() : undefined
                  }
                  isLoadingMore={isLoadingMore}
                />
              </>
            ) : (
              <Loading />
            )
          }
          unauthorized={<Forbidden />}
        />
      </Paper>
      {apiEmployees && apiLeaveTypes && (
        <GenerateNewYearLimitsModal
          open={isGenerateModalOpen}
          onClose={() => setIsGenerateModalOpen(false)}
          employees={apiEmployees.items}
          leaveTypes={apiLeaveTypes.items}
          onLimitsGenerated={handleLimitsGenerated}
        />
      )}
    </>
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
