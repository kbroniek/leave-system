import { useCallback } from "react";
import { useTranslation } from "react-i18next";
import { ShowNotification } from "@toolpad/core/useNotifications";
import { AccountInfo } from "@azure/msal-browser";
import { callApi, callApiGet } from "../utils/ApiCall";

// Custom hook that provides API call functions with translation support
export const useApiCall = () => {
  const { t } = useTranslation();

  const callApiWithT = useCallback(
    async (
      url: string,
      method: "GET" | "POST" | "PUT",
      body: unknown,
      showNotification: ShowNotification,
      signal?: AbortSignal,
      account?: AccountInfo | null,
    ) => {
      return callApi(url, method, body, showNotification, signal, account, t);
    },
    [t],
  );

  const callApiGetWithT = useCallback(
    <T>(
      url: string,
      showNotification: ShowNotification,
      signal?: AbortSignal,
      account?: AccountInfo | null,
    ): Promise<T> => {
      return callApiGet<T>(url, showNotification, signal, account, t);
    },
    [t],
  );

  return {
    callApi: callApiWithT,
    callApiGet: callApiGetWithT,
  };
};
