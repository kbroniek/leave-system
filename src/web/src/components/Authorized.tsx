import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";
import { callApiGet } from "../utils/ApiCall";
import { useNotifications } from "@toolpad/core/useNotifications";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [userRoles, setUserRoles] = useState<RoleType[] | undefined>(undefined);
  const { instance } = useMsal();
  const notifications = useNotifications();

  // Fetch roles from API
  useEffect(() => {
    const fetchUserRoles = async () => {
      if (!instance.getActiveAccount()) return;

      try {
        const response = await callApiGet<{ roles: RoleType[] }>(
          "/roles/me",
          notifications.show,
        );
        setUserRoles(response.roles);
      } catch (error) {
        console.error("Error fetching user roles:", error);
        setUserRoles([]);
      }
    };

    fetchUserRoles();
  }, [instance, notifications.show]);

  useEffect(() => {
    if (props.roles === "CurrentUser") {
      setIsAuthorized(
        props.userId === instance.getActiveAccount()?.idTokenClaims?.sub,
      );
    } else {
      setIsAuthorized(isInRole(props.roles, userRoles));
    }
  }, [instance, props, userRoles]);

  return (
    <>
      {isAuthorized ? (props.children ?? props.authorized) : props.unauthorized}
    </>
  );
};

function isInRole(
  roles: RoleType[],
  claimRoles: RoleType[] | undefined,
): boolean {
  return Array.isArray(claimRoles)
    ? !!claimRoles.find((c) => roles.includes(c))
    : false;
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
