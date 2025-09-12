import { useEffect, useState, useCallback } from "react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { loginRequest } from "../../authConfig";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Loading, LoadingAuth } from "../../components/Loading";
import { ManageUsersTable } from "./ManageUsersTable";
import { useNotifications } from "@toolpad/core/useNotifications";
import {
  callApi,
  callApiGet,
  ifErrorAcquireTokenRedirect,
} from "../../utils/ApiCall";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import { UserDto, UsersDto } from "../dtos/UsersDto";
import { useTranslation } from "react-i18next";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const notifications = useNotifications();
  const { t } = useTranslation();
  const [apiUsers, setApiUsers] = useState<UsersDto | undefined>();
  const [isUpdating, setIsUpdating] = useState(false);

  const handleUserChange = useCallback(
    async (user: UserDto): Promise<void> => {
      setIsUpdating(true);
      try {
        const response = await callApi(
          `/roles/${user.id}`,
          "PUT",
          {
            id: user.id,
            roles: user.roles,
          },
          notifications.show,
        );

        if (response.status === 200) {
          notifications.show(t("Roles is updated successfully"), {
            severity: "success",
            autoHideDuration: 3000,
          });

          // Update local state to reflect the change
          setApiUsers((prevUsers) =>
            prevUsers
              ? {
                  ...prevUsers,
                  items: prevUsers.items.map((existingUser) =>
                    existingUser.id === user.id ? user : existingUser,
                  ),
                }
              : prevUsers,
          );
        }
      } catch (error) {
        console.error("Error updating user:", error);
        notifications.show(t("Failed to update user roles"), {
          severity: "error",
          autoHideDuration: 5000,
        });
      } finally {
        setIsUpdating(false);
      }
    },
    [notifications, t],
  );

  const handleLoadingError = useCallback(
    (error: unknown) => {
      console.error("Error loading users:", error);
      notifications.show(t("Failed to load users"), {
        severity: "error",
        autoHideDuration: 5000,
      });
    },
    [notifications, t],
  );

  const handleDataLoad = useCallback((data: UsersDto) => {
    setApiUsers(data);
  }, []);

  useEffect(() => {
    if (inProgress === InteractionStatus.None) {
      callApiGet<UsersDto>("/users", notifications.show)
        .then(handleDataLoad)
        .catch((error) => {
          handleLoadingError(error);
          ifErrorAcquireTokenRedirect(error, instance);
        });
    }
  }, [
    inProgress,
    handleDataLoad,
    handleLoadingError,
    instance,
    notifications.show,
  ]);
  return apiUsers ? (
    <Authorized
      roles={["UserAdmin", "GlobalAdmin"]}
      authorized={
        <ManageUsersTable
          users={apiUsers.items}
          userOnChange={handleUserChange}
          isUpdating={isUpdating}
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
