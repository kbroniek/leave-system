import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";
import Stack from '@mui/material/Stack';
import Typography from "@mui/material/Typography";

import { LeaveRequestsTimeline } from "../ui-components/leave-requests/LeaveRequestsTimeline";
import { SignInButton } from "../ui-components/SignInButton";

export function Home() {
  return (
    <>
      <AuthenticatedTemplate>
          <LeaveRequestsTimeline />
      </AuthenticatedTemplate>

      <UnauthenticatedTemplate>
        <Typography variant="h6">
          <center>Please sign-in.</center>
          <SignInButton />
        </Typography>
      </UnauthenticatedTemplate>
    </>
  );
}