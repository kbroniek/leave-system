import { useParams } from "react-router-dom";
import { LeaveRequestDetails } from "./LeaveRequestDetails";
import Typography from "@mui/material/Typography";
import { Trans } from "react-i18next";

export function LeaveRequestDetailsPage() {
    const { id } = useParams();

    return id ? <LeaveRequestDetails leaveRequestId={id}/> : <Typography variant="h3"><Trans>404 Resource is not found</Trans></Typography>;
}