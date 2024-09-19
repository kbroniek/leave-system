import { Routes, Route, useNavigate } from "react-router-dom";
import { MsalProvider } from "@azure/msal-react";
import Grid from "@mui/material/Grid";

import type { AppProps } from "./types";

import { CustomNavigationClient } from "./NavigationClient";

import { PageLayout } from "./ui-components/PageLayout";
import { Home } from "./pages/Home";

export function App({ pca }: AppProps) {
  // The next 3 lines are optional. This is how you configure MSAL to take advantage of the router's navigate functions when MSAL redirects between pages in your app
  const navigate = useNavigate();
  const navigationClient = new CustomNavigationClient(navigate);
  pca.setNavigationClient(navigationClient);

  return (
    <MsalProvider instance={pca}>
      <PageLayout>
          <Grid container justifyContent="center">
              <Pages />
          </Grid>
      </PageLayout>
    </MsalProvider>
  );
}

function Pages() {
  return (
      <Routes>
          {/* <Route path="/profile" element={<Profile />} />
          <Route path="/profileWithMsal" element={<ProfileWithMsal />} />
          <Route path="/profileRawContext" element={<ProfileRawContext />} />
          <Route
              path="/profileUseMsalAuthenticationHook"
              element={<ProfileUseMsalAuthenticationHook />}
          />
          <Route path="/logout" element={<Logout />} /> */}
          <Route path="/" element={<Home />} />
      </Routes>
  );
}