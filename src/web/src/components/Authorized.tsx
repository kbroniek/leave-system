import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";
import { useUserRoles } from "../hooks/useUserRoles";
import { isInRoleInternal } from "../utils/roleUtils";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const { instance } = useMsal();
  const { roles: userRoles, isLoading } = useUserRoles({
    enabled: props.roles !== "CurrentUser", // Only fetch roles when needed
    cacheTimeout: 10 * 60 * 1000, // 10 minutes cache
    refetchOnWindowFocus: true,
  });

  useEffect(() => {
    if (props.roles === "CurrentUser") {
      setIsAuthorized(
        props.userId === instance.getActiveAccount()?.idTokenClaims?.sub,
      );
    } else {
      // Don't authorize until roles are loaded (unless loading fails and we get empty array)
      if (isLoading && userRoles === undefined) {
        setIsAuthorized(false);
      } else {
        setIsAuthorized(isInRoleInternal(props.roles, userRoles));
      }
    }
  }, [instance, props, userRoles, isLoading]);

  return (
    <>
      {isAuthorized ? (props.children ?? props.authorized) : props.unauthorized}
    </>
  );
};

// Moved utility functions to separate file to avoid fast refresh warnings

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
