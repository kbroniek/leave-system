import { UnauthenticatedTemplate } from "@azure/msal-react";
import Typography from "@mui/material/Typography";

import { LeaveRequestsTimeline } from "../ui-components/leave-requests/LeaveRequestsTimeline";
import { SignInButton } from "../ui-components/SignInButton";
import { Authorized } from "../components/Authorized";
import { Forbidden } from "../components/Forbidden";

export function Home() {
  return (
    <>
      <Authorized
        roles={["GlobalAdmin", "Employee", "DecisionMaker"]}
        authorized={<LeaveRequestsTimeline />}
        unauthorized={<Forbidden />}
      />

      <UnauthenticatedTemplate>
        <Typography variant="h6">
          <center>Please sign-in.</center>
          <SignInButton />
        </Typography>
      </UnauthenticatedTemplate>
    </>
  );
}
