import { useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import { Trans } from "react-i18next";

export function Logout() {
    const { instance } = useMsal();

    useEffect(() => {
        instance.logoutRedirect({
            account: instance.getActiveAccount(),
        })
    }, [ instance ]);

    return (
        <div><Trans>Logout</Trans></div>
    )
}
