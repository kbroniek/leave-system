import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import Typography from "@mui/material/Typography";

import { LeaveRequestsTimeline } from "../ui-components/leave-requests/LeaveRequestsTimeline";
import { SignInButton } from "../ui-components/SignInButton";
import { Authorized } from "../components/Authorized";
import { Forbidden } from "../components/Forbidden";
import { Trans } from "react-i18next";

export function Home() {
  return (
    <>
      <AuthenticatedTemplate>
        <Authorized
          roles={["GlobalAdmin", "Employee", "DecisionMaker"]}
          authorized={<LeaveRequestsTimeline />}
          unauthorized={<Forbidden />}
        />
      </AuthenticatedTemplate>

      <UnauthenticatedTemplate>
        <Typography variant="h6">
          <center><Trans>Please sign-in.</Trans></center>
          <SignInButton />
        </Typography>
      </UnauthenticatedTemplate>
    </>
  );
}
