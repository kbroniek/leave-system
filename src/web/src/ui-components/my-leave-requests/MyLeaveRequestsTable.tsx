import Typography from "@mui/material/Typography";
import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import Divider from "@mui/material/Divider";
import Box from "@mui/material/Box";
import { DataGrid } from "@mui/x-data-grid/DataGrid";
import { GridRenderCellParams } from "@mui/x-data-grid/models/params/gridCellParams";
import { DurationFormatter } from "../utils/DurationFormatter";
import CircularProgress from "@mui/material/CircularProgress";
import { gridClasses } from "@mui/x-data-grid/constants";
import styled from "@mui/material/styles/styled";
import { alpha } from "@mui/material/styles";
import { GridValidRowModel } from "@mui/x-data-grid/models/gridRows";
import { LeaveStatusDto } from "../dtos/LeaveStatusDto";
import { useState } from "react";
import { LeaveRequestDetailsDialog } from "../leave-request-details/LeaveRequestDetailsDialog";

const ODD_OPACITY = 0.2;
export const MyLeaveRequestsTable = (params: {
  leaveRequests: LeaveRequestDto[] | undefined;
  leaveTypes: LeaveTypeDto[] | undefined;
  leaveStatuses: LeaveStatusDto[] | undefined;
}): JSX.Element => {
  const [openDialog, setOpenDialog] = useState(false);
  const [showLeaveRequestId, setShowLeaveRequestId] = useState<string | undefined>(undefined);
  const handleClickOpen = () => {
    setOpenDialog(true);
  };

  const handleClose = () => {
    setOpenDialog(false);
  };
  const RenderDays = (
    props: Readonly<
      GridRenderCellParams<GridValidRowModel | LeaveRequestDto, LeaveRequestDto>
    >,
  ): JSX.Element => {
    if (!props.row) {
      return <>undefined</>;
    }
    return (
      <>
        {DurationFormatter.format(props.row.duration, props.row.workingHours)}
      </>
    );
  };
  const RenderStatus = (
    props: Readonly<
      GridRenderCellParams<GridValidRowModel | LeaveRequestDto, LeaveRequestDto>
    >,
  ): JSX.Element => {
    if (!props.row) {
      return <>undefined</>;
    }
    if (!params.leaveStatuses) {
      return <CircularProgress size="20px" />;
    }
    const leaveStatus = params.leaveStatuses.find(
      (x) => x.leaveRequestStatus === props.row.status,
    );
    if (!leaveStatus) {
      return <>{props.row.status}</>;
    }
    const style = {
      backgroundColor: leaveStatus.color ?? "transparent",
      width: "100%",
      height: "100%",
      alignContent: "center",
      textAlign: "center"
    };
    return <Typography sx={style}>{props.row.status}</Typography>;
  };

  const RenderType = (
    props: Readonly<
      GridRenderCellParams<GridValidRowModel | LeaveRequestDto, LeaveRequestDto>
    >,
  ): JSX.Element => {
    if (!props.row) {
      return <>undefined</>;
    }
    if (!params.leaveTypes) {
      return <CircularProgress size="20px" />;
    }
    const leaveType = params.leaveTypes.find(
      (x) => x.id === props.row.leaveTypeId,
    );
    if (!leaveType) {
      return <>{props.row.leaveTypeId}</>;
    }
    const style = {
      backgroundColor: leaveType.properties?.color ?? "transparent",
      width: "100%",
      height: "100%",
      alignContent: "center",
    };
    return <Typography sx={style}>{leaveType.name}</Typography>;
  };
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
  return (
    <Box sx={{ flexGrow: 1 }} margin={2}>
      <Typography variant="h5">Leave requests</Typography>
      <Divider />
      {!params.leaveRequests ? (
        <CircularProgress />
      ) : (
        <StripedDataGrid
          rowHeight={40}
          columns={[
            { field: "dateFrom", headerName: "From" },
            { field: "dateTo", headerName: "To" },
            {
              field: "days",
              headerName: "Days",
              renderCell: RenderDays,
              width: 30,
            },
            { field: "status", headerName: "Status", renderCell: RenderStatus },
            {
              field: "type",
              headerName: "Type",
              renderCell: RenderType,
              flex: 1,
            },
          ]}
          rows={params.leaveRequests}
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
          onRowClick={(item) => {setShowLeaveRequestId(item.row.id); handleClickOpen()}}
        />
      )}
      <LeaveRequestDetailsDialog
        open={openDialog}
        onClose={handleClose}
        leaveRequestId={showLeaveRequestId}
      />
    </Box>
  );
};
