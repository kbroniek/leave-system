import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";
import { isInRole, RoleType } from "../utils/roleUtils";
import { roleManager } from "../services/roleManager";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [roleUpdateTrigger, setRoleUpdateTrigger] = useState(0);
  const { instance } = useMsal();

  // Subscribe to role updates
  useEffect(() => {
    const unsubscribe = roleManager.addRoleUpdateListener(() => {
      // Trigger re-evaluation by updating the state
      setRoleUpdateTrigger((prev) => prev + 1);
    });

    return unsubscribe;
  }, []);

  useEffect(() => {
    if (props.roles === "CurrentUser") {
      setIsAuthorized(
        props.userId === instance.getActiveAccount()?.idTokenClaims?.sub,
      );
    } else {
      setIsAuthorized(isInRole(instance, props.roles));
    }
  }, [instance, props, roleUpdateTrigger]);

  return (
    <>
      {isAuthorized ? (props.children ?? props.authorized) : props.unauthorized}
    </>
  );
};

type RoleArgs =
  | { roles: RoleType[] }
  | { roles: "CurrentUser"; userId: string };

type AuthorizedProps = {
  authorized?: ReactElement;
  children?: JSX.Element | JSX.Element[];
  unauthorized?: ReactElement;
} & RoleArgs;
