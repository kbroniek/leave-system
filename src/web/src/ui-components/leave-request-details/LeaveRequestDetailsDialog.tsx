import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import { LeaveRequestDetails } from "./LeaveRequestDetails";

export interface LeaveRequestDetailsDialogProps {
    open: boolean;
    leaveRequestId: string
    onClose: () => void;
}

export function LeaveRequestDetailsDialog(props: LeaveRequestDetailsDialogProps) {
    const { onClose, open } = props;

    const handleClose = () => {
        onClose();
    };

    return (
        <Dialog onClose={handleClose} open={open}>
            <DialogTitle>Set backup account</DialogTitle>
            <LeaveRequestDetails leaveRequestId={props.leaveRequestId} />
        </Dialog>
    );
}