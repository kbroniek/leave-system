import { Suspense, lazy } from "react";
import { Routes, Route, useNavigate } from "react-router-dom";
import { MsalProvider } from "@azure/msal-react";
import Grid from "@mui/material/Grid";
import CircularProgress from "@mui/material/CircularProgress";
import Box from "@mui/material/Box";
import type { AppProps } from "./types";
import { CustomNavigationClient } from "./NavigationClient";
import { PageLayout } from "./ui-components/PageLayout";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterLuxon } from "@mui/x-date-pickers/AdapterLuxon";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";

// Lazy load routes for code splitting
const Home = lazy(() =>
  import("./pages/Home").then((module) => ({ default: module.Home }))
);
const Logout = lazy(() =>
  import("./pages/Logout").then((module) => ({ default: module.Logout }))
);
const Claims = lazy(() =>
  import("./pages/Claims").then((module) => ({ default: module.Claims }))
);
const MyLeaveRequests = lazy(() =>
  import("./ui-components/my-leave-requests/MyLeaveRequests").then(
    (module) => ({ default: module.MyLeaveRequests })
  )
);
const HrPanel = lazy(() =>
  import("./ui-components/hr-panel/HrPanel").then((module) => ({
    default: module.HrPanel,
  }))
);
const ManageUsers = lazy(() =>
  import("./ui-components/manage-users/ManageUsers").then((module) => ({
    default: module.ManageUsers,
  }))
);
const ManageLimits = lazy(() =>
  import("./ui-components/manage-limits/ManageLimits").then((module) => ({
    default: module.ManageLimits,
  }))
);
const LeaveRequestDetailsPage = lazy(() =>
  import("./ui-components/leave-request-details/LeaveRequestDetailsPage").then(
    (module) => ({
      default: module.LeaveRequestDetailsPage,
    })
  )
);
const MyLeaveRequestsPage = lazy(() =>
  import("./ui-components/my-leave-requests/MyLeaveRequestsPage").then(
    (module) => ({
      default: module.MyLeaveRequestsPage,
    })
  )
);

// Create a QueryClient instance
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
});

export function App({ pca }: Readonly<AppProps>) {
  // The next 3 lines are optional. This is how you configure MSAL to take advantage of the router's navigate functions when MSAL redirects between pages in your app
  const navigate = useNavigate();
  const navigationClient = new CustomNavigationClient(navigate);
  pca.setNavigationClient(navigationClient);

  return (
    <QueryClientProvider client={queryClient}>
      <LocalizationProvider dateAdapter={AdapterLuxon}>
        <MsalProvider instance={pca}>
          <PageLayout>
            <Grid container justifyContent="center">
              <Pages />
            </Grid>
          </PageLayout>
        </MsalProvider>
      </LocalizationProvider>
      {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
    </QueryClientProvider>
  );
}

// Loading fallback component
function LoadingFallback() {
  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="200px"
    >
      <CircularProgress />
    </Box>
  );
}

function Pages() {
  return (
    <Suspense fallback={<LoadingFallback />}>
      <Routes>
        <Route path="/logout" element={<Logout />} />
        <Route path="/" element={<Home />} />
        <Route path="/claims" element={<Claims />} />
        <Route path="/my-leaves" element={<MyLeaveRequests />} />
        <Route path="/user-leaves/:id" element={<MyLeaveRequestsPage />} />
        <Route path="/hr-panel" element={<HrPanel />} />
        <Route path="/users" element={<ManageUsers />} />
        <Route path="/limits" element={<ManageLimits />} />
        <Route path="/details/:id" element={<LeaveRequestDetailsPage />} />
      </Routes>
    </Suspense>
  );
}
