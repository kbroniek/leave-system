import { RoleType } from "../components/Authorized";

// Utility functions for role management - moved from Authorized component to avoid fast refresh warnings

export function isInRoleInternal(
  requiredRoles: RoleType[],
  claimRoles: RoleType[] | undefined,
): boolean {
  if (!Array.isArray(claimRoles) || claimRoles.length === 0) {
    return false;
  }
  // Check if user has any of the required roles or is a GlobalAdmin
  return (
    claimRoles.includes("GlobalAdmin") ||
    requiredRoles.some((role) => claimRoles.includes(role))
  );
}

export const roleTypeNames: string[] = [
  "Employee",
  "LeaveLimitAdmin",
  "LeaveTypeAdmin",
  "DecisionMaker",
  "HumanResource",
  "UserAdmin",
  "GlobalAdmin",
  "WorkingHoursAdmin",
];
