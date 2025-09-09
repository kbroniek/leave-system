import { useState, useEffect, useCallback, useRef } from "react";
import { useMsal } from "@azure/msal-react";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApiGet } from "../utils/ApiCall";
import { RoleType } from "../components/Authorized";
import { isInRoleInternal } from "../utils/roleUtils";

interface UserRolesCache {
  roles: RoleType[];
  timestamp: number;
  userId: string;
}

interface UseUserRolesOptions {
  cacheTimeout?: number; // in milliseconds, default 10 minutes
  refetchOnWindowFocus?: boolean;
  enabled?: boolean;
}

const CACHE_KEY = "user_roles_cache";
const DEFAULT_CACHE_TIMEOUT = 10 * 60 * 1000; // 10 minutes - standardized

// Global cache and promise to share across hook instances
let globalCache: UserRolesCache | null = null;
let ongoingFetch: Promise<RoleType[]> | null = null;
const cacheListeners = new Set<() => void>();

export const useUserRoles = (options: UseUserRolesOptions = {}) => {
  const {
    cacheTimeout = DEFAULT_CACHE_TIMEOUT,
    refetchOnWindowFocus = true,
    enabled = true,
  } = options;

  const { instance } = useMsal();
  const notifications = useNotifications();
  const [roles, setRoles] = useState<RoleType[] | undefined>(undefined);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);

  const getCurrentUserId = useCallback(() => {
    return (
      instance.getActiveAccount()?.idTokenClaims?.sub ||
      instance.getActiveAccount()?.localAccountId ||
      ""
    );
  }, [instance]);

  const isCacheValid = useCallback(
    (cache: UserRolesCache | null, userId: string): boolean => {
      if (!cache) return false;
      if (cache.userId !== userId) return false;
      return Date.now() - cache.timestamp < cacheTimeout;
    },
    [cacheTimeout],
  );

  const updateCache = useCallback((newRoles: RoleType[], userId: string) => {
    const newCache: UserRolesCache = {
      roles: newRoles,
      timestamp: Date.now(),
      userId,
    };

    globalCache = newCache;

    // Persist to localStorage for cross-tab consistency
    try {
      localStorage.setItem(CACHE_KEY, JSON.stringify(newCache));
    } catch (e) {
      console.warn("Failed to persist roles cache to localStorage:", e);
    }

    // Notify all hook instances
    cacheListeners.forEach((listener) => listener());
  }, []);

  const fetchUserRoles = useCallback(
    async (userId: string, signal?: AbortSignal): Promise<RoleType[]> => {
      if (!userId || !enabled) return [];

      // If there's already a fetch in progress, wait for it
      if (ongoingFetch) {
        try {
          return await ongoingFetch;
        } catch (error) {
          console.error("Error fetching user roles:", error);
          // If the ongoing fetch failed, we'll start a new one
          ongoingFetch = null;
        }
      }

      // Start a new fetch and store the promise
      ongoingFetch = (async (): Promise<RoleType[]> => {
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
          updateCache(newRoles, userId);
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
          // Clear the ongoing fetch promise
          ongoingFetch = null;
        }
      })();

      return ongoingFetch;
    },
    [enabled, notifications.show, updateCache],
  );

  const refetch = useCallback(async () => {
    const userId = getCurrentUserId();
    if (!userId) return;

    // Cancel any ongoing request
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    // Create new abort controller
    abortControllerRef.current = new AbortController();
    try {
      await fetchUserRoles(userId, abortControllerRef.current.signal);
    } catch {
      // Error is already handled in fetchUserRoles
    }
  }, [getCurrentUserId, fetchUserRoles]);

  const invalidateCache = useCallback(() => {
    globalCache = null;
    try {
      localStorage.removeItem(CACHE_KEY);
    } catch (e) {
      console.warn("Failed to remove roles cache from localStorage:", e);
    }
    refetch();
  }, [refetch]);

  // Load initial data
  useEffect(() => {
    if (!enabled) return;

    const userId = getCurrentUserId();
    if (!userId) {
      setRoles([]);
      return;
    }

    // Try to load from localStorage on first mount
    if (!globalCache) {
      try {
        const stored = localStorage.getItem(CACHE_KEY);
        if (stored) {
          globalCache = JSON.parse(stored);
        }
      } catch (e) {
        console.warn("Failed to load roles cache from localStorage:", e);
      }
    }

    // Check if cache is valid
    if (isCacheValid(globalCache, userId)) {
      setRoles(globalCache!.roles);
      return;
    }

    // Cache is invalid, fetch new data
    abortControllerRef.current = new AbortController();
    fetchUserRoles(userId, abortControllerRef.current.signal).catch(() => {
      // Error is already handled in fetchUserRoles
    });

    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [enabled, getCurrentUserId, isCacheValid, fetchUserRoles]);

  // Listen to cache updates from other hook instances
  useEffect(() => {
    const listener = () => {
      const userId = getCurrentUserId();
      if (globalCache && isCacheValid(globalCache, userId)) {
        setRoles(globalCache.roles);
      }
    };

    cacheListeners.add(listener);
    return () => {
      cacheListeners.delete(listener);
    };
  }, [getCurrentUserId, isCacheValid]);

  // Handle window focus refetch
  useEffect(() => {
    if (!refetchOnWindowFocus) return;

    const handleFocus = () => {
      const userId = getCurrentUserId();
      if (!userId || !enabled) return;

      if (!isCacheValid(globalCache, userId)) {
        refetch();
      }
    };

    window.addEventListener("focus", handleFocus);
    return () => window.removeEventListener("focus", handleFocus);
  }, [refetchOnWindowFocus, enabled, getCurrentUserId, isCacheValid, refetch]);

  return {
    roles,
    isLoading,
    error,
    refetch,
    invalidateCache,
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
