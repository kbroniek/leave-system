import { useState } from "react";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import Grid from "@mui/material/Grid";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { useNotifications } from "@toolpad/core/useNotifications";
import { DateTime, Duration } from "luxon";
import { useApiQuery } from "../../hooks/useApiQuery";
import { useApiMutation } from "../../hooks/useApiMutation";
import { LeaveLimitDto, LeaveLimitsDto } from "../dtos/LeaveLimitsDto";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { ManageLimitsTable, LeaveLimitCell } from "./ManageLimitsTable";
import { Loading } from "../../components/Loading";
import { useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";

interface GenerateNewYearLimitsModalProps {
  open: boolean;
  onClose: () => void;
  employees: EmployeeDto[];
  leaveTypes: LeaveTypeDto[];
  onLimitsGenerated: () => void;
}

export function GenerateNewYearLimitsModal({
  open,
  onClose,
  employees,
  leaveTypes,
  onLimitsGenerated,
}: GenerateNewYearLimitsModalProps) {
  const { t } = useTranslation();
  const notifications = useNotifications();
  const { inProgress } = useMsal();
  const [templateYear, setTemplateYear] = useState<number>(
    DateTime.local().year
  );
  const [shouldGenerate, setShouldGenerate] = useState<boolean>(false);

  // Use TanStack Query for generating limits
  const { data: generatedLimitsData, isLoading } = useApiQuery<LeaveLimitsDto>(
    ["generateLimits", templateYear],
    `/leavelimits/generate-new-year?year=${templateYear}`,
    { enabled: shouldGenerate && inProgress === InteractionStatus.None && open }
  );

  const generatedLimits = generatedLimitsData?.items ?? null;

  // Create mutation for saving limits
  const saveLimitsMutation = useApiMutation({
    onSuccess: () => {
      notifications.show(t("All limits saved successfully"), {
        severity: "success",
        autoHideDuration: 3000,
      });
      onLimitsGenerated();
      handleClose();
    },
    invalidateQueries: [["leaveLimits"]],
  });

  const handleGenerate = () => {
    setShouldGenerate(true);
  };

  const handleSaveAll = async () => {
    if (!generatedLimits) return;

    // Normalize all workingHours to ISO8601 format before sending
    const normalizedLimits = generatedLimits.map((limit) => {
      // If workingHours is not already in ISO8601 format, convert it
      let workingHoursValue = limit.workingHours;
      const duration = Duration.fromISO(workingHoursValue);
      if (!duration.isValid) {
        // Try to parse as a number (hours) and convert to ISO8601
        const hours = parseFloat(workingHoursValue);
        if (!isNaN(hours)) {
          workingHoursValue = Duration.fromObject({ hours }).toISO();
        }
      }
      return {
        ...limit,
        workingHours: workingHoursValue,
      };
    });

    saveLimitsMutation.mutate({
      url: `/leavelimits/batch`,
      method: "POST",
      body: normalizedLimits,
    });
  };

  const handleLimitChange = async (item: LeaveLimitCell): Promise<boolean> => {
    // Update the generated limits in memory for preview
    if (!generatedLimits) return false;

    const updatedLimits: LeaveLimitDto[] = generatedLimits.map((limit) => {
      if (limit.id === item.id) {
        const workingHours = item.workingHours ?? 8;
        return {
          ...limit,
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
      }
      return limit;
    });

    // TODO: Check this!
    // setGeneratedLimits(updatedLimits);
    if (updatedLimits.length > 0) {
      return true;
    }
    return true;
  };

  const handleClose = () => {
    setShouldGenerate(false);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xl" fullWidth>
      <DialogTitle>{t("Generate New Year Limits")}</DialogTitle>
      <DialogContent>
        <Grid
          container
          spacing={2}
          sx={{ marginBottom: 2, alignItems: "center" }}
        >
          <Typography variant="body1">{t("Template Year:")}</Typography>
          <TextField
            type="number"
            value={templateYear}
            onChange={(e) => setTemplateYear(Number(e.target.value))}
            sx={{ width: 120, marginLeft: 1 }}
          />
          <Button
            variant="contained"
            onClick={handleGenerate}
            disabled={isLoading}
            sx={{ marginLeft: 2 }}
          >
            {isLoading ? t("Generating...") : t("Generate")}
          </Button>
        </Grid>

        {isLoading && <Loading />}

        {generatedLimits && !isLoading && (
          <ManageLimitsTable
            employees={employees}
            leaveTypes={leaveTypes.filter((x) => x.state === "Active")}
            limits={generatedLimits.filter((x) => x.state === "Active")}
            limitOnChange={handleLimitChange}
          />
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={saveLimitsMutation.isPending}>
          {t("Cancel")}
        </Button>
        {generatedLimits && (
          <Button
            onClick={handleSaveAll}
            variant="contained"
            disabled={saveLimitsMutation.isPending}
          >
            {saveLimitsMutation.isPending
              ? t("Saving...")
              : t("Save All Limits")}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}
