import { useEffect } from "react";
import { BrowserUtils } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { Trans } from "react-i18next";

export function Logout() {
    const { instance } = useMsal();

    useEffect(() => {
        instance.logoutRedirect({
            account: instance.getActiveAccount(),
            onRedirectNavigate: () => !BrowserUtils.isInIframe()
        })
    }, [ instance ]);

    return (
        <div><Trans>Logout</Trans></div>
    )
}
