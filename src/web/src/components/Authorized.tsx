import { useMsal } from "@azure/msal-react";
import { useState, useEffect, ReactElement } from "react";
import { isInRole, RoleType } from "../utils/roleUtils";

export const Authorized = (props: AuthorizedProps) => {
  const [isAuthorized, setIsAuthorized] = useState(false);
  const { instance } = useMsal();
  useEffect(() => {
    if (props.roles === "CurrentUser") {
      setIsAuthorized(
        props.userId === instance.getActiveAccount()?.idTokenClaims?.sub,
      );
    } else {
      setIsAuthorized(isInRole(instance, props.roles));
    }
  }, [instance, props]);

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
