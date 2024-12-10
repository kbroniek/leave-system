import { useParams } from "react-router-dom";
import { LeaveRequestDetails } from "./LeaveRequestDetails";
import Typography from "@mui/material/Typography";

export function LeaveRequestDetailsPage() {
    const { id } = useParams();

    return id ? <LeaveRequestDetails leaveRequestId={id}/> : <Typography variant="h3">404 Resource is not found</Typography>;
}