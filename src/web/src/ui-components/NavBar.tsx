import { useState } from "react";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import Link from "@mui/material/Link";
import Typography from "@mui/material/Typography";
import IconButton from "@mui/material/IconButton";
import MenuIcon from "@mui/icons-material/Menu";
import WelcomeName from "./WelcomeName";
import SignInSignOutButton from "./SignInSignOutButton";
import { Link as RouterLink } from "react-router-dom";
import Drawer from "@mui/material/Drawer";
import List from "@mui/material/List";
import ListItemText from "@mui/material/ListItemText";
import { ListItemButton } from "@mui/material";

const NavBar = () => {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);

  return (
    <AppBar variant="outlined" position="static">
      <Toolbar>
        <IconButton
          edge="start"
          color="inherit"
          aria-label="menu"
          onClick={() => setIsDrawerOpen(true)}
        >
          <MenuIcon />
        </IconButton>
        <Typography sx={{ flexGrow: 1 }}>
            <Link component={RouterLink} to="/" color="inherit" variant="h6">Leave System</Link>
        </Typography>
        <WelcomeName />
        <SignInSignOutButton />

        <Drawer open={isDrawerOpen} onClose={() => setIsDrawerOpen(false)}>
          <List>
            <ListItemButton component={RouterLink} to="/" onClick={() => setIsDrawerOpen(false)}>
              <ListItemText primary="Home" />
            </ListItemButton>

            <ListItemButton component={RouterLink} to="/data" onClick={() => setIsDrawerOpen(false)}>
              <ListItemText primary="Call API" />
            </ListItemButton>
          </List>
        </Drawer>
      </Toolbar>
    </AppBar>
  );
};

export default NavBar;
