import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";
import { isInRole, RoleType } from "../utils/roleUtils";
import { roleManager } from "../services/roleManager";
import { CircularProgress, Box } from "@mui/material";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [roleUpdateTrigger, setRoleUpdateTrigger] = useState(0);
  const [isLoadingRoles, setIsLoadingRoles] = useState(false);
  const { instance } = useMsal();

  // Subscribe to role updates
  useEffect(() => {
    const unsubscribe = roleManager.addRoleUpdateListener(() => {
      // Trigger re-evaluation by updating the state
      setRoleUpdateTrigger((prev) => prev + 1);
    });

    return unsubscribe;
  }, []);

  // Subscribe to loading state changes
  useEffect(() => {
    const unsubscribe = roleManager.addLoadingStateListener((loading) => {
      setIsLoadingRoles(loading);
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

  // Show loading spinner if roles are being fetched and no roles exist yet
  if (isLoadingRoles) {
    return (
      props.loading ?? (
        <Box display="flex" justifyContent="center" alignItems="center" p={2}>
          <CircularProgress size={24} />
        </Box>
      )
    );
  }

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
  loading?: ReactElement;
} & RoleArgs;
