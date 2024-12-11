import { MsalAuthenticationResult } from "@azure/msal-react";
import Typography from "@mui/material/Typography";
import { Trans } from "react-i18next";

export const ErrorComponent: React.FC<MsalAuthenticationResult> = ({
  error,
}) => {
  console.error(error);
  return (
    <Typography variant="h6">
      <Trans i18nKey="errorOccurred">
        An Error Occurred: {error ? error.errorCode : "unknown error"}
      </Trans>
    </Typography>
  );
};
