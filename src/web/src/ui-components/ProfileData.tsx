import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import ListItemAvatar from "@mui/material/ListItemAvatar";
import Avatar from "@mui/material/Avatar";
import PersonIcon from '@mui/icons-material/Person';

export const ProfileData = (props: {apiData: {name: string}}) => {
    return (
        <List className="profileData">
            <NameListItem name={props.apiData.name} />
        </List>
    );
};

const NameListItem = (props: {name: string}) => (
    <ListItem>
        <ListItemAvatar>
            <Avatar>
                <PersonIcon />
            </Avatar>
        </ListItemAvatar>
        <ListItemText primary="Name" secondary={props.name}/>
    </ListItem>
);
