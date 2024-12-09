import {
  AccountInfo,
  InteractionRequiredAuthError,
  IPublicClientApplication,
} from "@azure/msal-browser";
import { loginRequest } from "../authConfig";
import { msalInstance } from "../main";
import { ShowNotification } from "@toolpad/core/useNotifications";

export function callApiGet<T>(
  url: string,
  showNotification: ShowNotification,
): Promise<T> {
  return callApi(url, "GET", undefined, showNotification).then((response) =>
    response.json(),
  );
}

export async function callApi(
  url: string,
  method: "GET" | "POST" | "PUT",
  body: unknown,
  showNotification: ShowNotification,
): Promise<Response> {
  const account = msalInstance.getActiveAccount();
  if (!account) {
    throw Error(
      "No active account! Verify a user has been signed in and setActiveAccount has been called.",
    );
  }

  const responseToken = await msalInstance.acquireTokenSilent({
    ...loginRequest,
    account: account,
  });
  const accessToken = responseToken.accessToken;

  const headers = new Headers();
  const bearer = `Bearer ${accessToken}`;

  headers.append("X-Authorization", bearer);
  if (body) {
    headers.append("Content-Type", "application/json");
  }

  const options = {
    method: method,
    headers: headers,
    body: body ? JSON.stringify(body) : undefined,
  };

  const response = await fetch(
    `${import.meta.env.VITE_REACT_APP_API_URL}${url}`,
    options,
  ).catch((error) => {
    showNotification(`Error: ${JSON.stringify(error)}`, {
      severity: "error",
      autoHideDuration: 3000,
    });
    return error;
  });

  if (response.status < 200 || response.status >= 300) {
    try
    {
      const errorBody = await response.json();
      showNotification(
        `Error: ${response.status}
          ${errorBody.title}
          ${errorBody.detail}
          ${response.statusText}`,
        {
          severity: "error",
          autoHideDuration: 3000,
        },
      );
    }
    catch(e) {
      console.error(e);
      showNotification(
        `Error: ${response.status} Something goes wrong.`,
        {
          severity: "error",
          autoHideDuration: 3000,
        },
      );
    }
  }
  return response;
}

export async function ifErrorAcquireTokenRedirect(
  error: unknown,
  instance: IPublicClientApplication,
) {
  if (error instanceof InteractionRequiredAuthError) {
    await instance.acquireTokenRedirect({
      ...loginRequest,
      account: instance.getActiveAccount() as AccountInfo | undefined,
    });
  }
}
