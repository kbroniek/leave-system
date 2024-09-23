import { loginRequest } from "../authConfig";
import { msalInstance } from "../main";

export async function callApi(url: string) {
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
        .catch(error => console.log(error));
}
