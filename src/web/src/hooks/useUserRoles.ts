import { useState, useEffect, useCallback, useRef } from "react";
import { useMsal } from "@azure/msal-react";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApiGet } from "../utils/ApiCall";
import { RoleType } from "../components/Authorized";
import { isInRoleInternal } from "../utils/roleUtils";

interface UseUserRolesOptions {
  enabled?: boolean;
}

export const useUserRoles = (options: UseUserRolesOptions = {}) => {
  const { enabled = true } = options;

  const { instance } = useMsal();
  const notifications = useNotifications();
  const [roles, setRoles] = useState<RoleType[] | undefined>(undefined);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

  const fetchUserRoles = useCallback(
    async (signal?: AbortSignal): Promise<RoleType[]> => {
      if (!enabled) return [];

      setIsLoading(true);
      setError(null);

      try {
        const response = await callApiGet<{ roles: RoleType[] }>(
          "/roles/me",
          notifications.show,
          signal,
        );

        if (signal?.aborted) throw new Error("Request aborted");

        const newRoles = response.roles || [];
        setRoles(newRoles);
        return newRoles;
      } catch (err) {
        if (signal?.aborted) throw err;

        const errorMessage =
          err instanceof Error ? err.message : "Failed to fetch user roles";
        setError(errorMessage);
        console.error("Error fetching user roles:", err);
        setRoles([]);
        throw err;
      } finally {
        if (!signal?.aborted) {
          setIsLoading(false);
        }
      }
    },
    [enabled, notifications.show],
  );

  const refetch = useCallback(async () => {
    // Cancel any ongoing request
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    // Create new abort controller
    abortControllerRef.current = new AbortController();
    try {
      await fetchUserRoles(abortControllerRef.current.signal);
    } catch {
      // Error is already handled in fetchUserRoles
    }
  }, [fetchUserRoles]);

  // Load initial data
  useEffect(() => {
    if (!enabled) return;

    const account = instance.getActiveAccount();
    if (!account) {
      setRoles([]);
      return;
    }

    // Fetch roles on mount
    abortControllerRef.current = new AbortController();
    fetchUserRoles(abortControllerRef.current.signal).catch(() => {
      // Error is already handled in fetchUserRoles
    });

    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [enabled, instance, fetchUserRoles]);

  return {
    roles,
    isLoading,
    error,
    refetch,
  };
};

// Helper hook for role checking
export const useHasRole = (requiredRoles: RoleType[]) => {
  const { roles, isLoading } = useUserRoles();

  const hasRole = useCallback(() => {
    return isInRoleInternal(requiredRoles, roles);
  }, [roles, requiredRoles]);

  return {
    hasRole: hasRole(),
    isLoading,
    roles,
  };
};
