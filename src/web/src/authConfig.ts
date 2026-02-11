import { Configuration, LogLevel, BrowserUtils } from "@azure/msal-browser";

/**
 * Enter here the user flows and custom policies for your B2C application
 * To learn more about user flows, visit: https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview
 * To learn more about custom policies, visit: https://docs.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview
 */
export const b2cPolicies = {
  authorities: {
    signUpSignIn: {
      authority: import.meta.env.VITE_REACT_APP_AUTHORITY_SIGNIN,
    },
  },
  authorityDomain: import.meta.env.VITE_REACT_APP_AUTHORITY_DOMAIN,
};

// Config object to be passed to Msal on creation
export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_REACT_APP_B2C_CLIENT_ID,
    authority: b2cPolicies.authorities.signUpSignIn.authority,
    knownAuthorities: [b2cPolicies.authorityDomain],
    redirectUri: "/",
    postLogoutRedirectUri: "/",
    onRedirectNavigate: (_url) => !BrowserUtils.isInIframe(),
  },
  cache: {
    cacheLocation: "localStorage",
  },
  system: {
    loggerOptions: {
      loggerCallback: (
        level: LogLevel,
        message: string,
        containsPii: boolean
      ) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            return;
          case LogLevel.Info:
            console.info(message);
            return;
          case LogLevel.Verbose:
            console.debug(message);
            return;
          case LogLevel.Warning:
            console.warn(message);
            return;
          default:
            return;
        }
      },
    },
  },
};

// Scopes you add here will be prompted for consent during login
export const loginRequest = {
  scopes: [import.meta.env.VITE_REACT_APP_B2C_SCOPE_API],
};

/**
 * Enter here the coordinates of your web API and scopes for access token request
 * The current application coordinates were pre-registered in a B2C tenant.
 */
export const apiConfig = {
  scopes: [import.meta.env.VITE_REACT_APP_B2C_SCOPE_API],
  uri: import.meta.env.VITE_REACT_APP_API_URL,
};
