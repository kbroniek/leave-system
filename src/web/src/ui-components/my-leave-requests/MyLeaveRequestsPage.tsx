import { useParams } from "react-router-dom";
import { MyLeaveRequests } from "./MyLeaveRequests";
import Typography from "@mui/material/Typography";
import { Trans } from "react-i18next";

export function MyLeaveRequestsPage() {
    const { id } = useParams();

    return id ? <MyLeaveRequests userId={id}/> : <Typography variant="h3"><Trans>404 Resource is not found</Trans></Typography>;
}