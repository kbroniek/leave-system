import { useParams } from "react-router-dom";
import { MyLeaveRequests } from "./MyLeaveRequests";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";

export function MyLeaveRequestsPage() {
    const { id } = useParams();
    const { t } = useTranslation();

    return id ? <MyLeaveRequests userId={id}/> : <Typography variant="h3">{t("404 Resource is not found")}</Typography>;
}