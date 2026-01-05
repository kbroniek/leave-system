import Dialog from "@mui/material/Dialog";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import IconButton from "@mui/material/IconButton";
import CloseIcon from "@mui/icons-material/Close";
import { DateTime } from "luxon";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { SubmitLeaveRequest } from "./SubmitLeaveRequest";
import { Trans } from "react-i18next";

interface SubmitLeaveRequestDialogProps {
  open: boolean;
  onClose: () => void;
  selectedDate?: DateTime;
  selectedEmployee?: EmployeeDto;
}

export function SubmitLeaveRequestDialog({
  open,
  onClose,
  selectedDate,
  selectedEmployee,
}: SubmitLeaveRequestDialogProps) {
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
        <SubmitLeaveRequest
          initialDate={selectedDate}
          initialEmployee={selectedEmployee}
          onSuccess={onClose}
        />
      </DialogContent>
    </Dialog>
  );
}
