import Dialog from "@mui/material/Dialog";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import CloseIcon from "@mui/icons-material/Close";
import { DateTime } from "luxon";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import {
  SubmitLeaveRequestForm,
  LeaveRequestFormModel,
} from "./SubmitLeaveRequestForm";
import { Trans } from "react-i18next";
import { SubmitHandler } from "react-hook-form";

interface SubmitLeaveRequestDialogProps {
  open: boolean;
  onClose: () => void;
  selectedDate?: DateTime;
  selectedEmployee?: EmployeeDto;
  leaveRequests?: LeaveRequestDto[];
  holidays?: HolidaysDto;
  leaveTypes?: LeaveTypeDto[];
  leaveLimits?: LeaveLimitDto[];
  employees?: EmployeeDto[];
  onSubmit: SubmitHandler<LeaveRequestFormModel>;
  onYearChanged: (year: string) => void;
  onUserIdChanged: (userId: string) => void;
}

export function SubmitLeaveRequestDialog({
  open,
  onClose,
  selectedDate,
  selectedEmployee,
  leaveRequests,
  holidays,
  leaveTypes,
  leaveLimits,
  employees,
  onSubmit,
  onYearChanged,
  onUserIdChanged,
}: SubmitLeaveRequestDialogProps) {
  const handleSubmit: SubmitHandler<LeaveRequestFormModel> = async (
    data,
    event,
  ) => {
    const result = await onSubmit(data, event);
    if (result === 201) {
      onClose();
    }
    return result;
  };

  // Prepare initial values for the form
  const initialValues: Partial<LeaveRequestFormModel> = {
    dateFrom: selectedDate,
    dateTo: selectedDate,
    onBehalf: selectedEmployee?.id,
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { minHeight: "400px" },
      }}
    >
      <DialogTitle>
        <Trans>Add leave request</Trans>
        <IconButton
          aria-label="close"
          onClick={onClose}
          sx={{
            position: "absolute",
            right: 8,
            top: 8,
            color: (theme) => theme.palette.grey[500],
          }}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <DialogContent dividers>
        <SubmitLeaveRequestForm
          leaveRequests={leaveRequests}
          holidays={holidays}
          leaveTypes={leaveTypes}
          leaveLimits={leaveLimits}
          employees={employees}
          onSubmit={handleSubmit}
          onYearChanged={onYearChanged}
          onUserIdChanged={onUserIdChanged}
          initialValues={initialValues}
          initialEmployee={selectedEmployee}
        />
      </DialogContent>
    </Dialog>
  );
}
