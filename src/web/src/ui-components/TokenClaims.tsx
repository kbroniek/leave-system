import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";

export const TokenClaims = (props: {tokenClaims: {[key: string]: unknown} | undefined}) => {
    console.log(props.tokenClaims);
    return (
        props.tokenClaims ?
        <List id="idTokenClaims">
            {Object.entries(props.tokenClaims).map(claim=> {
                return (
                    <ListItem key={`claim-${claim[0]}`}>
                        <ListItemText primary={claim[0]} secondary={Array.isArray(claim[1]) ? (claim[1] as string[]).join(",") : claim[1] as string}/>
                    </ListItem>
                );
            })}
        </List>
        : null
    );
};