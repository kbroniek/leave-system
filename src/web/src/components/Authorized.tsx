import { IPublicClientApplication } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";
import { roleManager } from "../services/roleManager";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const { instance } = useMsal();
  useEffect(() => {
    if (props.roles === "CurrentUser") {
      setIsAuthorized(
        props.userId === instance.getActiveAccount()?.idTokenClaims?.sub,
      );
    } else {
      const claimRoles = getRoles(instance);
      setIsAuthorized(isInRoleInternal(props.roles, claimRoles));
    }
  }, [instance, props]);

  return (
    <>
      {isAuthorized ? (props.children ?? props.authorized) : props.unauthorized}
    </>
  );
};

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
  return true;
  return isInRoleInternal(roles, getRoles(instance));
}

type RoleArgs =
  | { roles: RoleType[] }
  | { roles: "CurrentUser"; userId: string };

type AuthorizedProps = {
  authorized?: ReactElement;
  children?: JSX.Element | JSX.Element[];
  unauthorized?: ReactElement;
} & RoleArgs;

export type RoleType =
  | "Employee"
  | "LeaveLimitAdmin"
  | "LeaveTypeAdmin"
  | "DecisionMaker"
  | "HumanResource"
  | "UserAdmin"
  | "GlobalAdmin"
  | "WorkingHoursAdmin";

export const roleTypeNames = [
  "Employee",
  "LeaveLimitAdmin",
  "LeaveTypeAdmin",
  "DecisionMaker",
  "HumanResource",
  "UserAdmin",
  "GlobalAdmin",
  "WorkingHoursAdmin",
];
