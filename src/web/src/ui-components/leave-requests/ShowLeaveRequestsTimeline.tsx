import { LeaveRequestsResponseDto } from "./LeaveRequestsDto";
import { DateTime } from "luxon";
import { alpha, styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import { DataGrid } from "@mui/x-data-grid/DataGrid";
import { gridClasses } from "@mui/x-data-grid";
import {
  GridCellParams,
  GridColDef,
  GridColumnGroupingModel,
  GridRenderCellParams,
  GridValidRowModel,
} from "@mui/x-data-grid/models";
import Grid from "@mui/material/Grid2";
import { RenderLeaveRequests } from "./RenderLeaveRequests";
import { HolidaysDto } from "./HolidaysDto";
import { LeaveStatusesDto } from "../dtos/LeaveStatusDto";
import { RenderLeaveRequestModel } from "./RenderLeaveRequestModel";
import { LeaveTypesDto } from "../dtos/LeaveTypesDto";
import { UserDto } from "../dtos/UserDto";
import { LeaveRequestsTimelineTransformer } from "./LeaveRequestsTimelineTransformer";

export const rowHeight = 30;

export default function ShowLeaveRequestsTimeline(params: {
  leaveRequests: LeaveRequestsResponseDto,
  holidays: HolidaysDto,
  leaveStatuses: LeaveStatusesDto,
  leaveTypes: LeaveTypesDto
}): JSX.Element {
  // TODO: Get employee from api
  const employees: UserDto[] = [
    ...new Map(
      params.leaveRequests.items.map((item) => [item.createdBy.id, item.createdBy])
    ).values(),
  ];
  const leaveStatusesActive = params.leaveStatuses.items.filter(x => x.state === "Active");
  const leaveTypesActive = params.leaveTypes.items.filter(x => x.state === "Active");
  const transformer = new LeaveRequestsTimelineTransformer(employees, params.leaveRequests);
  const transformedData = transformer.transformToTable();
  const dates =
    transformedData.items.find(() => true)?.table.map((x) => x.date) ?? [];
  const rows = transformedData.items.map((x) => ({
    ...x.employee,
    ...x.table.reduce(
      (a, v) => ({
        ...a,
        [v.date.toISO()!]: {
          date: v.date,
          leaveRequests: v.leaveRequests,
          statuses: leaveStatusesActive,
          leaveTypes: leaveTypesActive
        } as RenderLeaveRequestModel,
      }),
      {}
    ),
  }));

  const transformedHolidays = params.holidays.items.map(x => DateTime.fromISO(x));
  const columns: GridColDef<GridValidRowModel[number]>[] = dates.map((x) => ({
    field: x.toISO()!,
    headerName: x.toFormat("dd"),
    width: 10,
    headerClassName: getDayCssClass(x, transformedHolidays),
    cellClassName: (params: GridCellParams<UserDto, { date: DateTime }>) =>
      !params.value ? "" : getDayCssClass(params.value.date, transformedHolidays),
    renderCell: (
      props: GridRenderCellParams<UserDto, RenderLeaveRequestModel>
    ) => RenderLeaveRequests(props),
  }));

  const groups: GridColumnGroupingModel = transformedData.header.map((x) => ({
    groupId: x.date.toFormat("LLLL"),
    children: x.days.map((x) => ({ field: x.toISO()! })),
  }));
  const ODD_OPACITY = 0.2;
  const StripedDataGrid = styled(DataGrid)(({ theme }) => ({
    ".MuiDataGrid-cell": {
      padding: 0
    },
    [`& .${gridClasses.columnHeader}, & .${gridClasses.cell}`]: {
      outline: 'transparent',
    },
    [`& .${gridClasses.columnHeader}:focus-within, & .${gridClasses.cell}:focus-within`]: {
      outline: 'none',
    },
    [`& .${gridClasses["row--borderBottom"]}`]: {
      "& .timeline-day.weekend": {
        backgroundColor: "#e0e006;",
      },
      "& .timeline-day.holiday": {
        backgroundColor: "#FFDD96;",
      },
      "& .timeline-day.today": {
        backgroundColor: "#c0c0c0;",
      },
    },
    [`& .${gridClasses.row}`]: {
      "& .timeline-day.date-from": {
        color: "#ff0000;",
      },
      "& .timeline-day.date-to": {
        color: "#ff00ff;",
      },
    },
    "& .timeline-day.today": {
      backgroundColor: "#c0c0c0;",
    },
    [`& .${gridClasses.row}.odd`]: {
      "& .timeline-day.weekend": {
        backgroundColor: "#e0e006;",
      },
      "& .timeline-day.holiday": {
        backgroundColor: "#FFDD96;",
      },
    },
    [`& .${gridClasses.row}.even`]: {
      "& .timeline-day.weekend": {
        backgroundColor: "#b8b82e;",
      },
      "& .timeline-day.holiday": {
        backgroundColor: "#d4b97c;",
      },
      backgroundColor: theme.palette.grey[200],
      "&:hover": {
        backgroundColor: alpha(theme.palette.primary.main, ODD_OPACITY),
        "@media (hover: none)": {
          backgroundColor: "transparent",
        },
      },
      "&.Mui-selected": {
        backgroundColor: alpha(
          theme.palette.primary.main,
          ODD_OPACITY + theme.palette.action.selectedOpacity
        ),
        "&:hover": {
          backgroundColor: alpha(
            theme.palette.primary.main,
            ODD_OPACITY +
              theme.palette.action.selectedOpacity +
              theme.palette.action.hoverOpacity
          ),
          // Reset on touch devices, it doesn't add specificity
          "@media (hover: none)": {
            backgroundColor: alpha(
              theme.palette.primary.main,
              ODD_OPACITY + theme.palette.action.selectedOpacity
            ),
          },
        },
      },
    },
  }));

  const EmployeeStripedDataGrid = styled(StripedDataGrid)(() => ({
    "& .MuiDataGrid-columnSeparator": {
      display: "none",
    },
    "& .MuiDataGrid-filler": {
      display: "none",
    },
  }));

  return (
    <Box sx={{ maxWidth: "100%", overflow: "auto", flexGrow: 1 }}>
      <Grid container spacing={0}>
        <Grid size={2}>
          <EmployeeStripedDataGrid
            rowHeight={rowHeight}
            columnHeaderHeight={rowHeight}
            rows={transformedData.items.map((x) => x.employee)}
            columns={[
              {
                field: "name",
                headerName: "",
                flex: 1,
              },
            ]}
            columnGroupingModel={[
              {
                groupId: "name",
                headerName: "",
                children: [{ field: "name" }],
              },
            ]}
            disableRowSelectionOnClick
            hideFooter={true}
            hideFooterPagination={true}
            hideFooterSelectedRowCount={true}
            disableColumnMenu
            disableColumnSorting
            disableColumnResize
            disableColumnFilter
            disableColumnSelector
            getRowClassName={(params) =>
              params.indexRelativeToCurrentPage % 2 === 0 ? "even" : "odd"
            }
          />
        </Grid>
        <Grid size={10}>
          <StripedDataGrid
            rowHeight={rowHeight}
            columnHeaderHeight={rowHeight}
            rows={rows}
            columns={columns}
            columnGroupingModel={groups}
            disableRowSelectionOnClick
            hideFooter={true}
            hideFooterPagination={true}
            hideFooterSelectedRowCount={true}
            disableColumnMenu
            disableColumnSorting
            disableColumnResize
            disableColumnFilter
            disableColumnSelector
            getRowClassName={(params) =>
              params.indexRelativeToCurrentPage % 2 === 0 ? "even" : "odd"
            }
          />
        </Grid>
      </Grid>
    </Box>
  );
}

function getDayCssClass(date: DateTime, holidays: DateTime[]): string {
  if (DateTime.local().startOf('day').equals(date)) {
    return "timeline-day today";
  }
  if (holidays.find(x => x.equals(date))) {
    return "timeline-day holiday";
  }
  return date.isWeekend ? "timeline-day weekend" : "timeline-day"
}
