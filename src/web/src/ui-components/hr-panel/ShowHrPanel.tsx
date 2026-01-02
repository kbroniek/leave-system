import { useNavigate } from "react-router-dom";
import { styled } from "@mui/material/styles";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { DataGrid } from "@mui/x-data-grid/DataGrid";
import { gridClasses } from "@mui/x-data-grid/constants";
import { alpha } from "@mui/material/styles";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import Grid from "@mui/material/Grid";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import { EmployeesFinder } from "../utils/EmployeesFinder";
import { HrTransformer } from "./HrTransformer";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import { GridRenderCellParams } from "@mui/x-data-grid/models/params";
import { GridValidRowModel } from "@mui/x-data-grid/models";
import DoDisturbOnIcon from "@mui/icons-material/DoDisturbOn";
import { useTranslation } from "react-i18next";
import * as React from "react";

const ODD_OPACITY = 0.2;
const selectedXDaysNumber = 14;
export const ShowHrPanel = (
  params: Readonly<{
    leaveRequests: LeaveRequestDto[] | undefined;
    leaveTypes: LeaveTypeDto[] | undefined;
    leaveLimits: LeaveLimitDto[] | undefined;
    employees: EmployeeDto[] | undefined;
    holidays: string[] | undefined;
  }>
): React.ReactElement => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const employees = EmployeesFinder.get(params.leaveRequests, params.employees);

  const isEmployeeDisabled = React.useCallback(
    (employeeId: string): boolean => {
      const employee = params.employees?.find((e) => e.id === employeeId);
      return employee ? employee.accountEnabled === false : false;
    },
    [params.employees]
  );
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
    "& .MuiDataGrid-row:hover": {
      cursor: "pointer",
    },
  }));
  const holidayLeaveType = params.leaveTypes?.find(
    (x) => x.properties?.catalog === "Holiday"
  );
  const redirectUserDetails = (userId: string) => {
    navigate(`/user-leaves/${userId}`);
  };
  const RenderSelectedXDays = (
    props: Readonly<GridRenderCellParams<GridValidRowModel>>
  ): React.ReactElement => {
    if (!props.row) {
      return <></>;
    }
    return props.row["selectedXDays"] ? (
      <CheckCircleIcon sx={{ verticalAlign: "middle" }} color="success" />
    ) : (
      <DoDisturbOnIcon sx={{ verticalAlign: "middle" }} />
    );
  };
  const columns = [
    {
      field: "totalLimit",
      headerName: t("Total available vacation days"),
    },
    {
      field: "limit",
      headerName: t("Number of days for the current year"),
    },
    {
      field: "overdueLimit",
      headerName: t("Number of days for previous years"),
    },
    {
      field: "limitLeft",
      headerName: t("Number of remaining vacation days"),
    },
    {
      field: "leaveTaken",
      headerName: t("Number of days in approved vacation days"),
    },
    {
      field: "selectedXDays",
      headerName: `${selectedXDaysNumber} ${t("days selected")}`,
      renderCell: RenderSelectedXDays,
    },
  ].concat(
    params.leaveTypes
      ?.filter((x) => x.id !== holidayLeaveType?.id)
      .map((x) => ({
        field: x.id,
        headerName: x.name,
      })) ?? []
  );
  const transformer = new HrTransformer(
    employees,
    params.leaveRequests,
    params.holidays,
    params.leaveTypes,
    params.leaveLimits,
    selectedXDaysNumber
  );
  const hrPanelData = transformer.transform();
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
                  renderCell: (cellParams) => {
                    const employeeId = cellParams.row.id as string;
                    const employeeName = cellParams.value as string;
                    const isDisabled = isEmployeeDisabled(employeeId);

                    const cellContent = (
                      <Typography
                        component="span"
                        sx={{
                          textDecoration: isDisabled ? "line-through" : "none",
                          color: isDisabled ? "text.disabled" : "inherit",
                          opacity: isDisabled ? 0.6 : 1,
                        }}
                      >
                        {employeeName}
                      </Typography>
                    );

                    if (isDisabled) {
                      return (
                        <Tooltip title={t("This user is disabled")} arrow>
                          <Box
                            component="span"
                            sx={{ display: "inline-block", width: "100%" }}
                          >
                            {cellContent}
                          </Box>
                        </Tooltip>
                      );
                    }

                    return cellContent;
                  },
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
              disableColumnSorting
              getRowClassName={(params) =>
                params.indexRelativeToCurrentPage % 2 === 0 ? "even" : "odd"
              }
              onRowClick={(e) => redirectUserDetails(e.id as string)}
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
              rows={hrPanelData}
              disableRowSelectionOnClick
              hideFooter={true}
              hideFooterPagination={true}
              hideFooterSelectedRowCount={true}
              disableColumnMenu
              disableColumnFilter
              disableColumnSelector
              disableColumnSorting
              getRowClassName={(params) =>
                params.indexRelativeToCurrentPage % 2 === 0 ? "even" : "odd"
              }
              onRowClick={(e) => redirectUserDetails(e.id as string)}
            />
          </Grid>
        </Grid>
      )}
    </Box>
  );
};
