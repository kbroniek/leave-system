import { PublicClientApplication, EventType } from "@azure/msal-browser";
import type { EventMessage, AccountInfo } from "@azure/msal-browser";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "./styles/theme";
import { NotificationsProvider } from "@toolpad/core/useNotifications";
import { msalConfig } from "./authConfig";
import { App } from "./App";
import { roleManager } from "./services/roleManager";

import "./i18n";
import "./index.css";

export const msalInstance = new PublicClientApplication(msalConfig);

msalInstance.initialize().then(() => {
  // Default to using the first account if no account is active on page load
  const accounts = msalInstance.getAllAccounts();
  if (!msalInstance.getActiveAccount() && accounts.length > 0) {
    msalInstance.setActiveAccount(accounts[0]);
  }

  msalInstance.addEventCallback(async (event: EventMessage) => {
    if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
      const account = event.payload as AccountInfo;
      msalInstance.setActiveAccount(account);

      // Fetch user roles after successful login
      try {
        const userId = account.homeAccountId || account.localAccountId;
        // Note: We don't have access to the translation function here,
        // so we pass undefined for the t parameter
        await roleManager.fetchAndSetRoles(
          userId,
          undefined,
          undefined,
          account,
          undefined,
        );
      } catch (error) {
        console.error("Failed to fetch roles after login:", error);
        // Don't prevent the login flow if role fetching fails
      }
    }
    if (event.eventType === EventType.ACQUIRE_TOKEN_FAILURE) {
      console.error(event.eventType, event.error);
      msalInstance.logoutRedirect({ account: msalInstance.getActiveAccount() });
    }
  });

  createRoot(document.getElementById("root") as HTMLElement).render(
    <StrictMode>
      <BrowserRouter>
        <ThemeProvider theme={theme}>
          <NotificationsProvider>
            <App pca={msalInstance} />
          </NotificationsProvider>
        </ThemeProvider>
      </BrowserRouter>
    </StrictMode>,
  );
});
