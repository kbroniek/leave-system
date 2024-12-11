import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";
import Typography from "@mui/material/Typography";

import { LeaveRequestsTimeline } from "../ui-components/leave-requests/LeaveRequestsTimeline";
import { SignInButton } from "../ui-components/SignInButton";
import { Authorized } from "../components/Authorized";
import { Forbidden } from "../components/Forbidden";
import { useTranslation } from "react-i18next";

export function Home() {
  const { t } = useTranslation();
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
          <center>{t("Please sign-in.")}</center>
          <SignInButton />
        </Typography>
      </UnauthenticatedTemplate>
    </>
  );
}
