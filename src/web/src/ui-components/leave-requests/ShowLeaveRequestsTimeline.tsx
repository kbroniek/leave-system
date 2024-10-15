import { LeaveRequestDto, LeaveRequestsResponseDto } from "./LeaveRequestsDto";
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
import { LeaveRequest } from "./LeaveRequestModel";
import { RenderLeaveRequests as renderLeaveRequests } from "./RenderLeaveRequests";
import { HolidaysDto } from "./HolidaysDto";

export const rowHeight = 30;

export default function ShowLeaveRequestsTimeline(params: {
  leaveRequests: LeaveRequestsResponseDto,
  holidays: HolidaysDto
}
) {
  // TODO: Get employee from api
  const employees: Employee[] = [
    ...new Map(
      params.leaveRequests.items.map((item) => [item.createdBy.id, item.createdBy])
    ).values(),
  ];
  const transformedData = transformToTable(params.leaveRequests, employees);
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
        },
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
    cellClassName: (params: GridCellParams<Employee, { date: DateTime }>) =>
      !params.value ? "" : getDayCssClass(params.value.date, transformedHolidays),
    renderCell: (
      props: GridRenderCellParams<Employee, { date: DateTime, leaveRequests: LeaveRequest[] }>
    ) => {
      return renderLeaveRequests(props)
    },
  }));

  const groups: GridColumnGroupingModel = transformedData.header.map((x) => ({
    groupId: x.date.toFormat("LLLL"),
    children: x.days.map((x) => ({ field: x.toISO()! })),
  }));
  const ODD_OPACITY = 0.2;
  const StripedDataGrid = styled(DataGrid)(({ theme }) => ({
    [".MuiDataGrid-cell"]: {
      padding: 0
    },
    [`& .${gridClasses["row--borderBottom"]}`]: {
      "& .timeline-day.weekend": {
        backgroundColor: "#e0e006;",
      },
      "& .timeline-day.holiday": {
        backgroundColor: "#FFDD96;",
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
  if(holidays.find(x => x.equals(date))) {
    return "timeline-day holiday";
  }
  return date.isWeekend ? "timeline-day weekend" : "timeline-day"
}

function buildDateTime(leaveRequests: LeaveRequestDto[]): LeaveRequest[] {
  return leaveRequests.map(
    (x) =>
      ({
        ...x,
        dateFrom: DateTime.fromISO(x.dateFrom),
        dateTo: DateTime.fromISO(x.dateTo),
        //TODO parse duration
      } as LeaveRequest)
  );
}

function transformToTable(
  leaveRequestsResponse: LeaveRequestsResponseDto,
  employees: Employee[]
): UserLeaveRequestTableCollection {
  const dateFrom = DateTime.fromISO(leaveRequestsResponse.search.dateFrom);
  const dateTo = DateTime.fromISO(leaveRequestsResponse.search.dateTo);
  // Ensure startDay is before endDay
  if (dateFrom > dateTo) {
    throw new Error("endDay must be after startDay");
  }

  const leaveRequests = buildDateTime(leaveRequestsResponse.items);
  return {
    items: employees.map((x) => ({
      employee: {
        id: x.id,
        name: x.name,
      },
      table: transformLeaveRequest(
        leaveRequests.filter((lr) => lr.createdBy.id === x.id),
        dateFrom,
        dateTo
      ),
    })),
    header: transformHeader(dateFrom, dateTo),
  };
}

function transformLeaveRequest(
  leaveRequests: LeaveRequest[],
  dateFrom: DateTime,
  dateTo: DateTime
): LeaveRequestTable[] {
  const dateSequence = createDatesSequence(dateFrom, dateTo);

  return dateSequence.map((currentDate) => ({
    date: currentDate,
    leaveRequests: leaveRequests.filter(
      (lr) => lr.dateFrom <= currentDate && lr.dateTo >= currentDate
    ),
  }));
}
function createDatesSequence(dateFrom: DateTime, dateTo: DateTime): DateTime[] {
  let currentDate = dateFrom;
  const sequence: DateTime[] = [];
  // Max one year
  for (
    let i = 0;
    i < 365 && currentDate <= dateTo;
    ++i, currentDate = currentDate.plus({ days: 1 })
  ) {
    sequence.push(currentDate);
  }
  return sequence;
}
function transformHeader(dateFrom: DateTime, dateTo: DateTime): HeaderTable[] {
  const headerTable: HeaderTable[] = [];
  const daysLeftDateFrom =
    getDaysInMonth(dateFrom.year, dateFrom.month) - dateFrom.day;
  headerTable.push({
    date: dateFrom,
    days: createDatesSequence(
      dateFrom,
      dateFrom.plus({ days: daysLeftDateFrom })
    ),
  });
  for (
    let currentDate = DateTime.fromObject({
      year: dateFrom.year,
      month: dateFrom.month,
      day: 1,
    }).plus({ month: 1 });
    currentDate.month < dateTo.month;
    currentDate = currentDate.plus({ month: 1 })
  ) {
    const daysLeft = getDaysInMonth(currentDate.year, currentDate.month) - 1;
    headerTable.push({
      date: currentDate,
      days: createDatesSequence(
        currentDate,
        currentDate.plus({ days: daysLeft })
      ),
    });
  }
  if (dateFrom.month != dateTo.month) {
    headerTable.push({
      date: dateTo,
      days: createDatesSequence(
        DateTime.fromObject({ year: dateTo.year, month: dateTo.month, day: 1 }),
        dateTo
      ),
    });
  }
  return headerTable;
}

function getDaysInMonth(year: number, month: number): number {
  return DateTime.fromObject({ year, month }).daysInMonth!;
}
interface UserLeaveRequestTableCollection {
  items: UserLeaveRequestTable[];
  header: HeaderTable[];
}
interface UserLeaveRequestTable {
  employee: {
    name: string;
    id: string;
  };
  table: LeaveRequestTable[];
}
interface LeaveRequestTable {
  date: DateTime;
  leaveRequests: LeaveRequest[];
}

interface Employee {
  name: string;
  id: string;
}

interface HeaderTable {
  days: DateTime[];
  date: DateTime;
}
