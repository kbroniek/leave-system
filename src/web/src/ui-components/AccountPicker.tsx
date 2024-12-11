import { useMsal } from "@azure/msal-react";
import Avatar from '@mui/material/Avatar';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemAvatar from '@mui/material/ListItemAvatar';
import ListItemText from '@mui/material/ListItemText';
import DialogTitle from '@mui/material/DialogTitle';
import Dialog from '@mui/material/Dialog';
import PersonIcon from '@mui/icons-material/Person';
import AddIcon from '@mui/icons-material/Add';
import { b2cPolicies, loginRequest } from "../authConfig";
import { AccountInfo } from '@azure/msal-browser';
import { Trans, useTranslation } from "react-i18next";

export const AccountPicker = (props: {onClose: (event: AccountInfo | null) => void, open: boolean}) => {
    const { instance, accounts } = useMsal();
    const { onClose, open } = props;
    const { t } = useTranslation();

    const handleListItemClick = (account: AccountInfo | null) => {
        instance.setActiveAccount(account);
        if (!account) {
            instance.loginRedirect({
                ...loginRequest,
                prompt: "login"
            })
        } else {
            // To ensure account related page attributes update after the account is changed
            window.location.reload();
        }

        onClose(account);
    };

    return (
        <Dialog onClose={(event: AccountInfo) => onClose(event)} aria-labelledby="simple-dialog-title" open={open}>
          <DialogTitle id="simple-dialog-title"><Trans>Set active account</Trans></DialogTitle>
          <List>
            {accounts.filter((account) => account.idTokenClaims?.['tfp'] !== b2cPolicies.names.signUpSignIn)
              .map((account) => (
                <ListItem onClick={() => handleListItemClick(account)} key={account.homeAccountId}>
                  <ListItemAvatar>
                    <Avatar>
                      <PersonIcon />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText primary={account.name ?? account.idTokenClaims?.signInName as string} secondary={account.username} />
                </ListItem>
            ))}

            <ListItem autoFocus onClick={() => handleListItemClick(null)}>
              <ListItemAvatar>
                <Avatar>
                  <AddIcon />
                </Avatar>
              </ListItemAvatar>
              <ListItemText primary={t("Add account")} />
            </ListItem>
          </List>
        </Dialog>
      );
};