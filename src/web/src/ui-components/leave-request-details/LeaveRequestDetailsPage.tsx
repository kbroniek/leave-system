import { useParams } from "react-router-dom";
import { LeaveRequestDetails } from "./LeaveRequestDetails";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";

export function LeaveRequestDetailsPage() {
    const { t } = useTranslation();
    const { id } = useParams();

    return id ? <LeaveRequestDetails leaveRequestId={id}/> : <Typography variant="h3">{t("404 Resource is not found")}</Typography>;
}