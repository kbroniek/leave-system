import { useState } from "react";
import { useMsal } from "@azure/msal-react";
import IconButton from "@mui/material/IconButton";
import AccountCircle from "@mui/icons-material/AccountCircle";
import MenuItem from "@mui/material/MenuItem";
import Menu from "@mui/material/Menu";
import { AccountPicker } from "./AccountPicker";
import Tooltip from "@mui/material/Tooltip";

export const SignOutButton = () => {
  const { instance } = useMsal();
  const [accountSelectorOpen, setAccountSelectorOpen] = useState(false);

  const [anchorEl, setAnchorEl] = useState<null | EventTarget & HTMLButtonElement>(null);
  const open = Boolean(anchorEl);

  const handleLogout = (logoutType: string) => {
    setAnchorEl(null);

    if (logoutType === "popup") {
      instance.logoutPopup();
    } else if (logoutType === "redirect") {
      instance.logoutRedirect();
    }
  };

  const handleAccountSelection = () => {
    setAnchorEl(null);
    setAccountSelectorOpen(true);
  };

  const handleClose = () => {
    setAccountSelectorOpen(false);
  };
  return (
    <div>
      <Tooltip title="Open settings">
      <IconButton
        onClick={(event) => setAnchorEl(event.currentTarget)}
        color="inherit"
      >
        <AccountCircle />
      </IconButton>
      </Tooltip>
      <Menu
        id="menu-appbar"
        sx={{ mt: '45px' }}
        anchorEl={anchorEl}
        anchorOrigin={{
          vertical: "top",
          horizontal: "right",
        }}
        keepMounted
        transformOrigin={{
          vertical: "top",
          horizontal: "right",
        }}
        open={Boolean(open)}
        onClose={() => setAnchorEl(null)}
      >
        <MenuItem onClick={() => handleAccountSelection()} key="switchAccount">
          Switch Account
        </MenuItem>
        <MenuItem onClick={() => handleLogout("redirect")} key="logoutRedirect">
          Logout
        </MenuItem>
      </Menu>
      <AccountPicker open={accountSelectorOpen} onClose={handleClose} />
    </div>
  );
};
