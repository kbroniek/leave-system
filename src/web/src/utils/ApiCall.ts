import { AccountInfo, InteractionRequiredAuthError, IPublicClientApplication } from "@azure/msal-browser";
import { loginRequest } from "../authConfig";
import { msalInstance } from "../main";

export async function callApi<T>(url: string): Promise<T> {
    const account = msalInstance.getActiveAccount();
    if (!account) {
        throw Error("No active account! Verify a user has been signed in and setActiveAccount has been called.");
    }

    const response = await msalInstance.acquireTokenSilent({
        ...loginRequest,
        account: account
    });
    const accessToken = response.accessToken;

    const headers = new Headers();
    const bearer = `Bearer ${accessToken}`;

    headers.append("X-Authorization", bearer);

    const options = {
        method: "GET",
        headers: headers
    };

    return fetch(`${import.meta.env.VITE_REACT_APP_API_URL}${url}`, options)
        .then(response => response.json())
        //TODO: Error handling
        .catch(error => console.log(error));
}

export async function ifErrorAcquireTokenRedirect(error: unknown, instance: IPublicClientApplication) {
    if (error instanceof InteractionRequiredAuthError) {
        instance.acquireTokenRedirect({
          ...loginRequest,
          account: instance.getActiveAccount() as AccountInfo | undefined,
        });
      }
}