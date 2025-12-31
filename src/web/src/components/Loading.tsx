import CircularProgress from "@mui/material/CircularProgress";
import LinearProgress from "@mui/material/LinearProgress";
import Typography from "@mui/material/Typography";

export const LoadingAuth = () => (<Loading />);
export const Loading = (props: {
  linearProgress?: boolean;
  label?: string;
}) => {
  return (
    <>
      <Typography variant="caption" gutterBottom sx={{ display: 'block' }}>{props.label}</Typography>
      {props.linearProgress ? (
        <LinearProgress />
      ) : (
        <CircularProgress sx={{ padding: 5 }} />
      )}
    </>
  );
};
