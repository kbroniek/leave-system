import { MsalAuthenticationResult } from "@azure/msal-react";
import Typography from "@mui/material/Typography";

export const ErrorComponent: React.FC<MsalAuthenticationResult> = ({error}) => {
    return <Typography variant="h6">An Error Occurred: {error?.errorCode}</Typography>;
}