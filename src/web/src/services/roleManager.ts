import { RoleType } from "../utils/roleUtils";
import { callApiGet } from "../utils/ApiCall";
import { ShowNotification } from "@toolpad/core/useNotifications";
const ROLES_LOCAL_STORAGE_KEY = "user.roles";
class RoleManager {
  private eventListeners: Array<() => void> = [];

  // Add listener for role updates
  addRoleUpdateListener(callback: () => void): () => void {
    this.eventListeners.push(callback);
    // Return unsubscribe function
    return () => {
      const index = this.eventListeners.indexOf(callback);
      if (index > -1) {
        this.eventListeners.splice(index, 1);
      }
    };
  }

  // Notify all listeners that roles have been updated
  private notifyRoleUpdate(): void {
    this.eventListeners.forEach((callback) => {
      try {
        callback();
      } catch (error) {
        console.error("Error in role update listener:", error);
      }
    });
  }

  /**
   * Fetches roles from the API after login
   */
  async fetchAndSetRoles(
    userId: string,
    showNotification?: ShowNotification,
    signal?: AbortSignal,
  ): Promise<RoleType[]> {
    try {
      // If we have a notification function, use it, otherwise use a simple no-op notification function
      const notificationFn: ShowNotification = showNotification || (() => "");

      const response = await callApiGet<{ roles: RoleType[] }>(
        "/roles/me",
        notificationFn,
        signal,
      );

      const roles = response.roles || [];
      // Store roles in localStorage per userId
      if (userId) {
        try {
          localStorage.setItem(
            `${ROLES_LOCAL_STORAGE_KEY}.${userId}`,
            JSON.stringify(roles),
          );
          // Notify all listeners that roles have been updated
          this.notifyRoleUpdate();
        } catch (e) {
          console.warn("Failed to store roles in localStorage:", e);
        }
      }
      return roles;
    } catch (error) {
      console.error("Error fetching roles:", error);
      throw error;
    }
  }

  getRoles(userId: string): RoleType[] {
    try {
      const roles = localStorage.getItem(
        `${ROLES_LOCAL_STORAGE_KEY}.${userId}`,
      );
      return roles ? JSON.parse(roles) : [];
    } catch (error) {
      console.error("Error getting roles from localStorage:", error);
      throw error;
    }
  }
}

// Export a singleton instance
export const roleManager = new RoleManager();
