import {
  AccountInfo,
  InteractionRequiredAuthError,
  IPublicClientApplication,
} from "@azure/msal-browser";
import { loginRequest } from "../authConfig";
import { msalInstance } from "../main";
import { ShowNotification } from "@toolpad/core/useNotifications";
import { ApiError } from "../types";
import {
  getErrorMessage,
  getErrorSeverity,
  getAutoHideDuration,
} from "./errorHandling";
import type { TFunction } from "i18next";

// Enhanced API call functions that support error code translation
export function callApiGet<T>(
  url: string,
  showNotification: ShowNotification,
  signal?: AbortSignal,
  account?: AccountInfo | null,
  t?: TFunction,
): Promise<T> {
  return callApi(
    url,
    "GET",
    undefined,
    showNotification,
    signal,
    account,
    t,
  ).then((response) => response?.json());
}

export async function callApi(
  url: string,
  method: "GET" | "POST" | "PUT",
  body: unknown,
  showNotification: ShowNotification,
  signal?: AbortSignal,
  account?: AccountInfo | null,
  t?: TFunction,
): Promise<Response> {
  account = account || msalInstance.getActiveAccount();
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
    signal: signal,
  };

  const response = await fetch(
    `${import.meta.env.VITE_REACT_APP_API_URL}${url}`,
    options,
  ).catch((error) => {
    console.error("callApi error", error);
    const errorMessage = t
      ? t("errors.networkError", { message: error.message })
      : `Error: ${error.message}`;
    showNotification(errorMessage, {
      severity: "error",
      autoHideDuration: 3000,
    });
    return error;
  });

  if (response.status < 200 || response.status >= 300) {
    await handleApiErrorWithTranslation(response, showNotification, t);
  }
  return response;
}

// Enhanced error handler that uses translation function
async function handleApiErrorWithTranslation(
  response: Response,
  showNotification: ShowNotification,
  t?: TFunction,
): Promise<void> {
  try {
    console.warn("API error occurred:", response);

    const errorBody = (await response.json()) as ApiError;

    // Extract error information
    const error: ApiError = {
      ...errorBody,
      status: response.status,
      errorCode: errorBody.errorCode || "UNKNOWN_ERROR",
    };

    // Get translated error message
    const errorMessage = t
      ? getErrorMessage(error, t)
      : error.title || error.detail || "An unexpected error occurred";
    const severity = getErrorSeverity(error.errorCode);
    const autoHideDuration = getAutoHideDuration(error.errorCode);

    // Show the notification
    showNotification(errorMessage, {
      severity,
      autoHideDuration,
    });
  } catch (parseError) {
    console.error("Failed to parse error response:", parseError);

    // Fallback error handling
    const fallbackMessage = t
      ? t("errors.parseError")
      : `Error ${response.status}: ${response.statusText || "Something went wrong"}`;
    showNotification(fallbackMessage, {
      severity: "error",
      autoHideDuration: 5000,
    });
  }
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
