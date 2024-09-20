import { useMsal } from "@azure/msal-react";
import Button from "@mui/material/Button";
import { loginRequest } from "../authConfig";

export const SignInButton = () => {
    const { instance } = useMsal();

    const handleLogin = (loginType: string) => {
        if (loginType === "popup") {
            instance.loginPopup(loginRequest);
        } else if (loginType === "redirect") {
            instance.loginRedirect(loginRequest);
        }
    }

    return (
        <div>
            <Button
                onClick={() => handleLogin("redirect")}
                color="inherit"
            >
                Login
            </Button>
        </div>
    )
};