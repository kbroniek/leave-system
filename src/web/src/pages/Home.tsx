import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";
import Stack from '@mui/material/Stack';
import Typography from "@mui/material/Typography";

import { LeaveRequestTimeline } from "../ui-components/leave-request/LeaveRequestTimeline";
import { SignInButton } from "../ui-components/SignInButton";

export function Home() {
  return (
    <>
      <AuthenticatedTemplate>
        <Stack direction="column" spacing={2}>
          <LeaveRequestTimeline />
        </Stack>
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