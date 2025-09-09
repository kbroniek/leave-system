import { useState, useEffect, useCallback, useRef } from "react";
import { useMsal } from "@azure/msal-react";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApiGet } from "../utils/ApiCall";
import { RoleType } from "../components/Authorized";

interface UserRolesCache {
  roles: RoleType[];
  timestamp: number;
  userId: string;
}

interface UseUserRolesOptions {
  cacheTimeout?: number; // in milliseconds, default 5 minutes
  refetchOnWindowFocus?: boolean;
  enabled?: boolean;
}

const CACHE_KEY = "user_roles_cache";
const DEFAULT_CACHE_TIMEOUT = 5 * 60 * 1000; // 5 minutes

// Global cache to share across hook instances
let globalCache: UserRolesCache | null = null;
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
    async (userId: string, signal?: AbortSignal) => {
      if (!userId || !enabled) return;

      setIsLoading(true);
      setError(null);

      try {
        const response = await callApiGet<{ roles: RoleType[] }>(
          "/roles/me",
          notifications.show,
          signal,
        );

        if (signal?.aborted) return;

        const newRoles = response.roles || [];
        updateCache(newRoles, userId);
        setRoles(newRoles);
      } catch (err) {
        if (signal?.aborted) return;

        const errorMessage =
          err instanceof Error ? err.message : "Failed to fetch user roles";
        setError(errorMessage);
        console.error("Error fetching user roles:", err);
        setRoles([]);
      } finally {
        if (!signal?.aborted) {
          setIsLoading(false);
        }
      }
    },
    [enabled, notifications.show, updateCache],
  );

  const refetch = useCallback(() => {
    const userId = getCurrentUserId();
    if (!userId) return;

    // Cancel any ongoing request
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    // Create new abort controller
    abortControllerRef.current = new AbortController();
    fetchUserRoles(userId, abortControllerRef.current.signal);
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
    fetchUserRoles(userId, abortControllerRef.current.signal);

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
    if (!roles || roles.length === 0) return false;
    return (
      requiredRoles.some((role) => roles.includes(role)) ||
      roles.includes("GlobalAdmin")
    );
  }, [roles, requiredRoles]);

  return {
    hasRole: hasRole(),
    isLoading,
    roles,
  };
};
