import styled from "@mui/material/styles/styled";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { UserDto } from "../dtos/UserDto";
import { DataGrid } from "@mui/x-data-grid/DataGrid";
import { gridClasses } from "@mui/x-data-grid/constants";
import { alpha } from "@mui/material/styles";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import Grid from "@mui/material/Grid2";
import { EmployeesFinder } from "../utils/EmployeesFinder";

const ODD_OPACITY = 0.2;
const selectedXDaysNumber = 14;
export const ShowHrPanel = (
  params: Readonly<{
    leaveRequests: LeaveRequestDto[] | undefined;
    leaveTypes: LeaveTypeDto[] | undefined;
    leaveLimits: LeaveLimitDto[] | undefined;
    employees: UserDto[] | undefined;
    holidays: string[] | undefined;
  }>,
): JSX.Element => {
  const employees = EmployeesFinder.get(params.leaveRequests, params.employees);
  const StripedDataGrid = styled(DataGrid)(({ theme }) => ({
    [`& .${gridClasses.row}.even`]: {
      backgroundColor: theme.palette.grey[200],
      "&:hover": {
        backgroundColor: alpha(theme.palette.primary.main, ODD_OPACITY),
        "@media (hover: none)": {
          backgroundColor: "transparent",
        },
      },
    },
  }));
  const holidayLeaveType = params.leaveTypes?.find(
    (x) => x.properties?.catalog === "Holiday",
  );
  const columns = [
    {
      field: "totalLimit",
      headerName: "Total available vacation days",
    },
    {
      field: "limit",
      headerName: "Number of days for the current year",
    },
    {
      field: "overdueLimit",
      headerName: "Number of days for previous years",
    },
    {
      field: "limitLeft",
      headerName: "Number of remaining vacation days",
    },
    {
      field: "leaveTaken",
      headerName: "Number of days in approved vacation days",
    },
    {
      field: "selectedXDays",
      headerName: `${selectedXDaysNumber} days selected`,
    },
  ].concat(
    params.leaveTypes
      ?.filter((x) => x.id !== holidayLeaveType?.id)
      .map((x) => ({
        field: x.id,
        headerName: x.name,
      })) ?? [],
  );
  return (
    <Box sx={{ flexGrow: 1 }} margin={2}>
      {!params.leaveRequests ? (
        <CircularProgress />
      ) : (
        <Grid container spacing={0}>
          <Grid size={2}>
            <StripedDataGrid
              rowHeight={40}
              columnHeaderHeight={100}
              columns={[
                {
                  field: "name",
                  headerName: "",
                  flex: 1,
                },
              ]}
              rows={employees}
              disableRowSelectionOnClick
              hideFooter={true}
              hideFooterPagination={true}
              hideFooterSelectedRowCount={true}
              disableColumnMenu
              disableColumnFilter
              disableColumnSelector
              getRowClassName={(params) =>
                params.indexRelativeToCurrentPage % 2 === 0 ? "even" : "odd"
              }
            />
          </Grid>
          <Grid size={10}>
            <StripedDataGrid
              sx={{
                "& .MuiDataGrid-columnHeaderTitle": {
                  whiteSpace: "normal",
                  lineHeight: "normal",
                },
              }}
              columnHeaderHeight={100}
              rowHeight={40}
              columns={columns}
              rows={employees}
              disableRowSelectionOnClick
              hideFooter={true}
              hideFooterPagination={true}
              hideFooterSelectedRowCount={true}
              disableColumnMenu
              disableColumnFilter
              disableColumnSelector
              getRowClassName={(params) =>
                params.indexRelativeToCurrentPage % 2 === 0 ? "even" : "odd"
              }
            />
          </Grid>
        </Grid>
      )}
    </Box>
  );
};
