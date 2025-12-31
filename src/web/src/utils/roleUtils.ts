import { IPublicClientApplication } from "@azure/msal-browser";
import { roleManager } from "../services/roleManager";

function getRoles(instance: IPublicClientApplication): RoleType[] | undefined {
  const claims = instance.getActiveAccount();
  if (!claims) {
    console.error("No active account found. getRoles failed.");
    return [];
  }
  const roles = roleManager.getRoles(
    claims.homeAccountId || claims.localAccountId,
  );
  return roles;
}

function isInRoleInternal(
  roles: RoleType[],
  claimRoles: RoleType[] | undefined,
): boolean {
  return Array.isArray(claimRoles)
    ? !!claimRoles.find((c) => roles.includes(c))
    : false;
}

export function isInRole(
  instance: IPublicClientApplication,
  roles: RoleType[],
) {
  return isInRoleInternal(roles, getRoles(instance));
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

export type RoleType =
  | "Employee"
  | "LeaveLimitAdmin"
  | "LeaveTypeAdmin"
  | "DecisionMaker"
  | "HumanResource"
  | "UserAdmin"
  | "GlobalAdmin"
  | "WorkingHoursAdmin";
