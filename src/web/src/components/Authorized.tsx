import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const { instance } = useMsal();
  const claims = instance.getActiveAccount()?.idTokenClaims;
  const claimRoles = claims?.roles as RoleType[] | undefined;
  useEffect(() => {
    if(Array.isArray(claimRoles)) {
      const foundRole = claimRoles.find(c => props.roles.includes(c));
      setIsAuthorized(!!foundRole);
    }
    else {
      setIsAuthorized(false)
    }
  }, [claimRoles, props.roles]);

  return <>{isAuthorized ? props.authorized : props.unauthorized}</>;
};

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
