import { useEffect } from "react";
import { BrowserUtils } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { useTranslation } from "react-i18next";

export function Logout() {
    const { instance } = useMsal();
    const { t } = useTranslation();

    useEffect(() => {
        instance.logoutRedirect({
            account: instance.getActiveAccount(),
            onRedirectNavigate: () => !BrowserUtils.isInIframe()
        })
    }, [ instance ]);

    return (
        <div>{t("Logout")}</div>
    )
}
