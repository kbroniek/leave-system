import { Routes, Route, useNavigate } from "react-router-dom";
import { MsalProvider } from "@azure/msal-react";
import Grid from "@mui/material/Grid2";
import type { AppProps } from "./types";
import { CustomNavigationClient } from "./NavigationClient";
import { PageLayout } from "./ui-components/PageLayout";
import { Home } from "./pages/Home";
import { Logout } from "./pages/Logout";
import { Claims } from "./pages/Claims";
import { SubmitLeaveRequest } from "./ui-components/submit-leave-request/SubmitLeaveRequest";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterLuxon } from '@mui/x-date-pickers/AdapterLuxon'
import { MyLeaveRequests } from "./ui-components/my-leave-requests/MyLeaveRequests";
import { HrPanel } from "./ui-components/hr-panel/HrPanel";
import { ManageUsers } from "./ui-components/manage-users/ManageUsers";
import { ManageLimits } from "./ui-components/manage-limits/ManageLimits";


export function App({ pca }: Readonly<AppProps>) {
  // The next 3 lines are optional. This is how you configure MSAL to take advantage of the router's navigate functions when MSAL redirects between pages in your app
  const navigate = useNavigate();
  const navigationClient = new CustomNavigationClient(navigate);
  pca.setNavigationClient(navigationClient);

  return (
    <LocalizationProvider dateAdapter={AdapterLuxon}>
      <MsalProvider instance={pca}>
        <PageLayout>
            <Grid container justifyContent="center">
                <Pages />
            </Grid>
        </PageLayout>
      </MsalProvider>
    </LocalizationProvider>
  );
}

function Pages() {
  return (
      <Routes>
          <Route path="/logout" element={<Logout />} />
          <Route path="/" element={<Home />} />
          <Route path="/claims" element={<Claims />} />
          <Route path="/submit-request" element={<SubmitLeaveRequest />} />
          <Route path="/my-leaves" element={<MyLeaveRequests />} />
          <Route path="/hr-panel" element={<HrPanel />} />
          <Route path="/users" element={<ManageUsers />} />
          <Route path="/limits" element={<ManageLimits />} />
      </Routes>
  );
}