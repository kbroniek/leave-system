import { useCallback } from "react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { loginRequest } from "../../authConfig";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Loading, LoadingAuth } from "../../components/Loading";
import { ManageUsersTable } from "./ManageUsersTable";
import { useNotifications } from "@toolpad/core/useNotifications";
import { useApiQuery } from "../../hooks/useApiQuery";
import { useApiMutation } from "../../hooks/useApiMutation";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import { UserDto, UsersDto } from "../dtos/UsersDto";
import { useTranslation } from "react-i18next";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const notifications = useNotifications();
  const { t } = useTranslation();

  // Use TanStack Query for fetching users
  const { data: apiUsers, isLoading } = useApiQuery<UsersDto>(
    ["users"],
    "/users",
    { enabled: inProgress === InteractionStatus.None }
  );

  // Use TanStack Query mutation for updating user roles
  const updateUserMutation = useApiMutation({
    onSuccess: (response, variables) => {
      if (response.status === 200) {
        notifications.show(t("Roles is updated successfully"), {
          severity: "success",
          autoHideDuration: 3000,
        });
      }
    },
    onError: () => {
      notifications.show(t("Failed to update user roles"), {
        severity: "error",
        autoHideDuration: 5000,
      });
    },
    invalidateQueries: [["users"]],
  });

  const handleUserChange = useCallback(
    async (user: UserDto): Promise<void> => {
      updateUserMutation.mutate({
        url: `/roles/${user.id}`,
        method: "PUT",
        body: {
          id: user.id,
          roles: user.roles,
        },
      });
    },
    [updateUserMutation],
  );

  if (isLoading) {
    return <Loading />;
  }

  return apiUsers ? (
    <Authorized
      roles={["UserAdmin", "GlobalAdmin"]}
      authorized={
        <ManageUsersTable
          users={apiUsers.items}
          userOnChange={handleUserChange}
          isUpdating={updateUserMutation.isPending}
        />
      }
      unauthorized={<Forbidden />}
    />
  ) : null;
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
