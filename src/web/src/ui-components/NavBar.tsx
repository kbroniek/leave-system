import * as React from "react";
import AppBar from "@mui/material/AppBar";
import Toolbar from "@mui/material/Toolbar";
import Link from "@mui/material/Link";
import Typography from "@mui/material/Typography";
import IconButton from "@mui/material/IconButton";
import MenuIcon from "@mui/icons-material/Menu";
import WelcomeName from "./WelcomeName";
import SignInSignOutButton from "./SignInSignOutButton";
import { Link as RouterLink } from "react-router-dom";
import Box from "@mui/material/Box";
import Menu from "@mui/material/Menu";
import Container from "@mui/material/Container";
import Button from "@mui/material/Button";
import MenuItem from "@mui/material/MenuItem";
import { Authorized, RoleType } from "../components/Authorized";
import { useTranslation } from "react-i18next";


function NavBar() {
  const { t } = useTranslation();
  const pages = [
    { title: t("Submit request"), link: "/submit-request", roles: ["DecisionMaker", "GlobalAdmin", "Employee"] as RoleType[] },
    { title: t("My leaves"), link: "/my-leaves", roles: ["Employee"] as RoleType[] },
    { title: t("HR Panel"), link: "hr-panel", roles: ["GlobalAdmin", "HumanResource"] as RoleType[] },
    { title: t("Users"), link: "/users", roles: ["UserAdmin", "GlobalAdmin"] as RoleType[] },
    { title: t("Manage limits"), link: "/limits", roles: ["GlobalAdmin"] as RoleType[] },
  ];
  const [anchorElNav, setAnchorElNav] = React.useState<null | HTMLElement>(
    null
  );

  const handleOpenNavMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorElNav(event.currentTarget);
  };

  const handleCloseNavMenu = () => {
    setAnchorElNav(null);
  };

  const style = {
    ".logo-image": {
      height: "40px",
      backgroundColor: "white",
      boxShadow: "0px 0px 11px silver",
      borderRadius: "5px",
    },
  };

  return (
    <AppBar position="static" sx={style}>
      <Container maxWidth="xl">
        <Toolbar disableGutters>
          <Link component={RouterLink} to="/" color="inherit">
            <img
              src="/logo.png"
              alt={t("company-title")}
              loading="lazy"
              className="logo-image"
            />
          </Link>

          <Box sx={{ flexGrow: 1, display: { xs: "flex", md: "none" } }}>
            <IconButton
              size="large"
              aria-label="account of current user"
              aria-controls="menu-appbar"
              aria-haspopup="true"
              onClick={handleOpenNavMenu}
              color="inherit"
            >
              <MenuIcon />
            </IconButton>
            <Menu
              id="menu-appbar"
              anchorEl={anchorElNav}
              anchorOrigin={{
                vertical: "bottom",
                horizontal: "left",
              }}
              keepMounted
              transformOrigin={{
                vertical: "top",
                horizontal: "left",
              }}
              open={Boolean(anchorElNav)}
              onClose={handleCloseNavMenu}
              sx={{ display: { xs: "block", md: "none" } }}
            >
              {pages.map((page) => (
                <Authorized
                  key={page.link}
                  roles={page.roles}
                  authorized={
                    <MenuItem
                      key={page.title}
                      onClick={handleCloseNavMenu}
                      component={RouterLink}
                      to={page.link}
                    >
                      <Typography sx={{ textAlign: "center" }}>{page.title}</Typography>
                    </MenuItem>}
                />
              ))}
            </Menu>
          </Box>
          <Box sx={{ flexGrow: 1, display: { xs: "none", md: "flex" } }}>
            {pages.map((page) => (
              <Authorized
                key={page.link}
                roles={page.roles}
                authorized={
                <Button
                  sx={{ my: 2, color: "white", display: "block" }}
                  key={page.title}
                  onClick={handleCloseNavMenu}
                  component={RouterLink}
                  to={page.link}
                >
                  {page.title}
                </Button>}
              />
            ))}
          </Box>
          <WelcomeName />
          <SignInSignOutButton />
        </Toolbar>
      </Container>
    </AppBar>
  );
}
export default NavBar;
