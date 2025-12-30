import { useMutation, useQueryClient, UseMutationOptions } from "@tanstack/react-query";
import { useMsal } from "@azure/msal-react";
import { useTranslation } from "react-i18next";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApi, ifErrorAcquireTokenRedirect } from "../utils/ApiCall";
import { AccountInfo } from "@azure/msal-browser";

/**
 * Custom hook for POST/PUT API calls using TanStack Query mutations
 */
export function useApiMutation<TData = unknown, TVariables = unknown>(
  options?: {
    onSuccess?: (data: Response, variables: TVariables) => void;
    onError?: (error: unknown, variables: TVariables) => void;
    invalidateQueries?: (string | number | undefined)[][];
    account?: AccountInfo | null;
  },
) {
  const { instance } = useMsal();
  const { t } = useTranslation();
  const notifications = useNotifications();
  const queryClient = useQueryClient();

  return useMutation<Response, unknown, { url: string; method: "POST" | "PUT"; body: TVariables }>({
    mutationFn: async ({ url, method, body }) => {
      const account = options?.account || instance.getActiveAccount();
      try {
        return await callApi(url, method, body, notifications.show, undefined, account, t);
      } catch (error) {
        await ifErrorAcquireTokenRedirect(error, instance);
        throw error;
      }
    },
    onSuccess: (data, variables) => {
      // Invalidate related queries if specified
      if (options?.invalidateQueries) {
        options.invalidateQueries.forEach((queryKey) => {
          queryClient.invalidateQueries({ queryKey });
        });
      }
      options?.onSuccess?.(data, variables.body);
    },
    onError: (error, variables) => {
      options?.onError?.(error, variables.body);
    },
  } as UseMutationOptions<Response, unknown, { url: string; method: "POST" | "PUT"; body: TVariables }>);
}

