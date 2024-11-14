import { IPublicClientApplication } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const { instance } = useMsal();
  const claimRoles = getRoles(instance)
  useEffect(() => {
    setIsAuthorized(isInRoleInternal(props.roles, claimRoles));
  }, [claimRoles, props.roles]);

  return <>{isAuthorized ? props.authorized : props.unauthorized}</>;
};

function getRoles(instance: IPublicClientApplication): RoleType[] | undefined {
  const claims = instance.getActiveAccount()?.idTokenClaims;
  const claimRoles = claims?.roles as RoleType[] | undefined;
  return claimRoles;
}

function isInRoleInternal(roles:  RoleType[], claimRoles: RoleType[] | undefined): boolean {
  return Array.isArray(claimRoles) ? !!claimRoles.find(c => roles.includes(c)) : false;
}

export function isInRole(instance: IPublicClientApplication, roles:  RoleType[]) {
  return isInRoleInternal(roles, getRoles(instance));
}

interface AuthorizedProps {
  authorized: ReactElement;
  unauthorized?: ReactElement;
  roles: RoleType[];
}

export type RoleType = "Employee" |
"LeaveLimitAdmin" |
"LeaveTypeAdmin" |
"DecisionMaker" |
"HumanResource" |
"UserAdmin" |
"GlobalAdmin" |
"WorkingHoursAdmin";
