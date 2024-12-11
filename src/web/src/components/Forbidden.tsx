import Typography from "@mui/material/Typography"
import { useTranslation } from "react-i18next";


export const Forbidden = () => {
    const { t } = useTranslation();
    return (
        <Typography variant="h2">{t("Error 403 Forbidden")}</Typography>
    )
}