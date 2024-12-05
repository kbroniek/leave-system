import { useEffect, useState } from "react";
import { InteractionStatus, InteractionType } from "@azure/msal-browser";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { loginRequest } from "../../authConfig";
import { ErrorComponent } from "../../components/ErrorComponent";
import { Loading, LoadingAuth } from "../../components/Loading";
import { ManageUsersTable } from "./ManageUsersTable";
import { useNotifications } from "@toolpad/core/useNotifications";
import { callApiGet, ifErrorAcquireTokenRedirect } from "../../utils/ApiCall";
import { Authorized } from "../../components/Authorized";
import { Forbidden } from "../../components/Forbidden";
import { UsersDto } from "../dtos/UsersDto";

const DataContent = () => {
  const { instance, inProgress } = useMsal();
  const notifications = useNotifications();
  const [apiUsers, setApiUsers] = useState<UsersDto | undefined>();

  useEffect(() => {
    if (inProgress === InteractionStatus.None) {
      callApiGet<UsersDto>("/users", notifications.show)
        .then((response) => setApiUsers(response))
        .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      //TODO: Wait for API
      //   callApiGet<RolesDto>("/roles", notifications.show)
      //     .then((response) => setApiUsers(response))
      //     .catch((e) => ifErrorAcquireTokenRedirect(e, instance));
      setApiUsers({
        items: [
          {
            id: "4e67827b-f93a-40c3-96f1-9193bdfac06d",
            name: "",
            firstName: "Olgierd",
            lastName: "Urbanek",
            department: "Lekarz",
            roles: ["DecisionMaker", "Employee"],
          },
          {
            id: "dca20276-2a30-43b0-89ce-3b0d0bd8501a",
            name: "",
            firstName: "Karol",
            lastName: "Volt",
            department: "Administrator",
            roles: ["GlobalAdmin"],
          },
          {
            id: "1769e6ea-a351-45bb-a518-a08f59fdde71",
            name: "",
            firstName: "Michał",
            lastName: "Pająk",
            department: "Fizjoterapia",
            roles: ["Employee"],
          },
          {
            id: "c188430f-3abd-4885-80f5-75e1b44f7b2b",
            name: "Robert Lewandowski",
            roles: ["Employee"],
          },
          {
            id: "beb60a1d-a7c5-49b5-b3af-f5e43b34d2ad",
            name: "Testowy",
            roles: ["Employee", "DecisionMaker"],
          },
          {
            id: "c73ae584-beb1-4036-8f04-6879d3b1955f",
            name: "",
            firstName: "Tomasz",
            lastName: "Sojka",
            roles: [],
          },
        ],
      });
    }
  }, [inProgress]);
  return apiUsers ?
    (<Authorized
      roles={["UserAdmin", "GlobalAdmin"]}
      authorized={<ManageUsersTable
        users={apiUsers.items} />}
      unauthorized={<Forbidden />}
    />
  ) : <Loading />;
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
