import { useEffect, useState } from "react";
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
import {
  callApi,
  callApiGet,
  ifErrorAcquireTokenRedirect,
} from "../../utils/ApiCall";
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

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const notifications = useNotifications();
  const { t } = useTranslation();
  const [apiEmployees, setApiEmployees] = useState<EmployeesDto | undefined>();
  const [apiLeaveTypes, setApiLeaveTypes] = useState<
    LeaveTypesDto | undefined
  >();
  const [apiLeaveLimits, setApiLeaveLimits] = useState<
    LeaveLimitsDto | undefined
  >();
  const [currentYear, setCurrentYear] = useState<number>(DateTime.local().year);
  const [isGenerateModalOpen, setIsGenerateModalOpen] =
    useState<boolean>(false);

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
    const response = await callApi(
      `/leavelimits/${dto.id}`,
      "PUT",
      dto,
      notifications.show
    );
    if (response.status === 200) {
      notifications.show(t("Limit is updated successfully"), {
        severity: "success",
        autoHideDuration: 3000,
      });
      return true;
    }
    return false;
  };

  const handleLimitsGenerated = () => {
    // Refresh the current limits data
    if (inProgress === InteractionStatus.None) {
      callApiGet<LeaveLimitsDto>(
        `/leavelimits?year=${currentYear}`,
        notifications.show
      )
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
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
  }, [inProgress, currentYear, instance, notifications.show]);

  useEffect(() => {
    if (inProgress === InteractionStatus.None) {
      callApiGet<LeaveLimitsDto>(
        `/leavelimits?year=${currentYear}`,
        notifications.show
      )
        .then((response) => setApiLeaveLimits(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [currentYear, inProgress, instance, notifications.show]);
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
                setApiLeaveLimits(undefined);
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
              <ManageLimitsTable
                employees={apiEmployees.items}
                leaveTypes={apiLeaveTypes.items.filter(
                  (x) => x.state === "Active"
                )}
                limits={apiLeaveLimits?.items.filter(
                  (x) => x.state === "Active"
                )}
                limitOnChange={handleLimitChange}
              />
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
