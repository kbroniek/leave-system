import { useEffect, useState } from "react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { loginRequest } from "../../authConfig";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Loading, LoadingAuth } from "../../components/Loading";
import { ManageUsersTable } from "./ManageUsersTable";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApi, callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import { UserDto, UsersDto } from "../dtos/UsersDto";
import { useTranslation } from "react-i18next";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const notifications = useNotifications();
  const { t } = useTranslation();
  const [apiUsers, setApiUsers] = useState<UsersDto | undefined>();

  const handleUserChange = async (user: UserDto): Promise<void> => {
    const response = await callApi(`/roles/${user.id}`, "PUT", {
      id: user.id,
      roles: user.roles
    }, notifications.show);
    if (response.status === 200) {
      notifications.show(t("Roles is updated successfully"), {
        severity: "success",
        autoHideDuration: 3000,
      });
    }
  };

  useEffect(() => {
    if (inProgress === InteractionStatus.None) {
      callApiGet<UsersDto>("/users", notifications.show)
        .then((response) => setApiUsers(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
    }
  }, [inProgress]);
  return apiUsers ? (
    <Authorized
      roles={["UserAdmin", "GlobalAdmin"]}
      authorized={
        <ManageUsersTable
          users={apiUsers.items}
          userOnChange={handleUserChange}
        />
      }
      unauthorized={<Forbidden />}
    />
  ) : (
    <Loading />
  );
};

export const ManageUsers = () => (
  <MsalAuthenticationTemplate
    interactionType={InteractionType.Redirect}
    authenticationRequest={loginRequest}
    errorComponent={ErrorComponent}
    loadingComponent={LoadingAuth}
  >
    <DataContent />
  </MsalAuthenticationTemplate>
);
