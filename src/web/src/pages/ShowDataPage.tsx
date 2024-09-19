import { useEffect, useState } from "react";

// Msal imports
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionStatus, InteractionType, InteractionRequiredAuthError, AccountInfo } from "@azure/msal-browser";
import { loginRequest } from "../authConfig";

// Sample app imports
import { ShowData } from "../ui-components/ShowData";
import { Loading } from "../ui-components/Loading";
import { ErrorComponent } from "../ui-components/ErrorComponent";
import { callApi } from "../utils/ApiCall";

// Material-ui imports
import Paper from "@mui/material/Paper";

const DataContent = () => {
    const { instance, inProgress } = useMsal();
    const [apiData, setApiData] = useState(null);

    useEffect(() => {
        if (!apiData && inProgress === InteractionStatus.None) {
            callApi().then(response => setApiData(response)).catch((e) => {
                if (e instanceof InteractionRequiredAuthError) {
                    instance.acquireTokenRedirect({
                        ...loginRequest,
                        account: instance.getActiveAccount() as AccountInfo | undefined
                    });
                }
            });
        }
    }, [inProgress, apiData, instance]);

    return (
        <Paper>
            { apiData ? <ShowData apiData={apiData} /> : null }
        </Paper>
    );
};

export function Data() {
    const authRequest = {
        ...loginRequest
    };

    return (
        <MsalAuthenticationTemplate 
            interactionType={InteractionType.Redirect} 
            authenticationRequest={authRequest} 
            errorComponent={ErrorComponent} 
            loadingComponent={Loading}
        >
            <DataContent />
        </MsalAuthenticationTemplate>
      )
};