import Dialog from "@mui/material/Dialog";
import DialogContent from "@mui/material/DialogContent";
import { LeaveRequestDetails } from "./LeaveRequestDetails";

export interface LeaveRequestDetailsDialogProps {
    open: boolean;
    leaveRequestId: string | undefined;
    onClose: () => void;
}

export function LeaveRequestDetailsDialog(props: LeaveRequestDetailsDialogProps) {
    const { onClose, open } = props;

    const handleClose = () => {
        onClose();
    };

    return (
        <Dialog 
            onClose={handleClose} 
            open={open}
            maxWidth="sm"
            fullWidth
        >
            <DialogContent dividers>
                {props.leaveRequestId ? <LeaveRequestDetails leaveRequestId={props.leaveRequestId} onClose={handleClose} /> : <></>}
            </DialogContent>
        </Dialog>
    );
}