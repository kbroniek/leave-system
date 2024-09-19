import { loginRequest } from "../authConfig";
import { msalInstance } from "../main";

export async function callApi(accessToken?: string) {
    if (!accessToken) {
        const account = msalInstance.getActiveAccount();
        console.log("account !!!!!!!", account);
        if (!account) {
            throw Error("No active account! Verify a user has been signed in and setActiveAccount has been called.");
        }
    
        const response = await msalInstance.acquireTokenSilent({
            ...loginRequest,
            account: account
        });
        accessToken = response.accessToken;
    }
    console.log("Access TOKEN!!!!!!!", accessToken);

    const headers = new Headers();
    const bearer = `Bearer ${accessToken}`;

    headers.append("X-Authorization", bearer);

    const options = {
        method: "GET",
        headers: headers
    };

    return fetch("https://lemon-cliff-0306f2803.5.azurestaticapps.net/api/leaverequests?dateFrom=2024-08-21&dateTo=2024-12-23", options)
        .then(response => response.json())
        .catch(error => console.log(error));
}
