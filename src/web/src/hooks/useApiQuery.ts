import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { useMsal } from "@azure/msal-react";
import { useTranslation } from "react-i18next";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../utils/ApiCall";
import { AccountInfo } from "@azure/msal-browser";

/**
 * Custom hook for GET API calls using TanStack Query
 */
export function useApiQuery<T>(
  queryKey: (string | number | undefined)[],
  url: string,
  options?: {
    enabled?: boolean;
    account?: AccountInfo | null;
    signal?: AbortSignal;
  },
) {
  const { instance } = useMsal();
  const { t } = useTranslation();
  const notifications = useNotifications();

  return useQuery<T>({
    queryKey,
    queryFn: async () => {
      const account = options?.account || instance.getActiveAccount();
      try {
        return await callApiGet<T>(
          url,
          notifications.show,
          options?.signal,
          account,
          t,
        );
      } catch (error) {
        await ifErrorAcquireTokenRedirect(error, instance);
        throw error;
      }
    },
    enabled: options?.enabled !== false,
    ...options,
  } as UseQueryOptions<T>);
}

