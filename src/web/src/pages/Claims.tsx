import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";

import Stack from '@mui/material/Stack';
import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";

import { TokenClaims } from "../ui-components/TokenClaims";
import { SignInButton } from "../ui-components/SignInButton";

export function Claims() {
  const { instance } = useMsal();

  return (
    <>
      <AuthenticatedTemplate>
        <Stack direction="column" spacing={2}>
          <Typography variant="body1">Claims in your ID token are shown below: </Typography>
          <Paper>
            {instance.getActiveAccount() ? <TokenClaims tokenClaims={instance.getActiveAccount()?.idTokenClaims} /> : null}
          </Paper>
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