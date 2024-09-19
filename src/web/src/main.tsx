import { PublicClientApplication, EventType } from "@azure/msal-browser";
import type { EventMessage, AuthenticationResult } from "@azure/msal-browser";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "./styles/theme";

import { msalConfig } from "./authConfig";

import { App } from "./App";

import "./index.css";

export const msalInstance = new PublicClientApplication(msalConfig);
await msalInstance.initialize();

// Optional - This will update account state if a user signs in from another tab or window
msalInstance.enableAccountStorageEvents();

// Default to using the first account if no account is active on page load
const accounts = msalInstance.getAllAccounts();
//TODO: Remove
console.log({ accounts, msalInstance });
if (!msalInstance.getActiveAccount() && accounts.length > 0) {
  // Account selection logic is app dependent. Adjust as needed for different use cases.
  msalInstance.setActiveAccount(accounts[0]);
}

msalInstance.addEventCallback((event: EventMessage) => {
  if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
    const payload = event.payload as AuthenticationResult;
    const account = payload.account;
    msalInstance.setActiveAccount(account);
  }
});

createRoot(document.getElementById("root") as HTMLElement).render(
  <StrictMode>
    <BrowserRouter>
      <ThemeProvider theme={theme}>
        <App pca={msalInstance} />
      </ThemeProvider>
    </BrowserRouter>
  </StrictMode>
);
