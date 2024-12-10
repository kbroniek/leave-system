import { useParams } from "react-router-dom";
import { MyLeaveRequests } from "./MyLeaveRequests";
import Typography from "@mui/material/Typography";

export function MyLeaveRequestsPage() {
    const { id } = useParams();

    return id ? <MyLeaveRequests userId={id}/> : <Typography variant="h3">404 Resource is not found</Typography>;
}