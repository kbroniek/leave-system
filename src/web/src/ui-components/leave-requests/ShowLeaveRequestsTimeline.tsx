import { LeaveRequestDto, LeaveRequestsResponseDto } from "./LeaveRequestsDto";
import { DateTime, Duration } from "luxon";
import Box from "@mui/material/Box";
import { DataGrid } from "@mui/x-data-grid/DataGrid";
import { GridColDef, GridColumnGroupingModel } from "@mui/x-data-grid/models";

export default function ShowLeaveRequestsTimeline(
  apiData: LeaveRequestsResponseDto
) {
  // TODO: Get employee from api
  const employees: Employee[] = [
    ...new Map(
      apiData.items.map((item) => [item.createdBy.id, item.createdBy])
    ).values(),
  ];
  const transformedData = transformToTable(apiData, employees);
  const dates =
    transformedData.items.find(() => true)?.table.map((x) => x.date) ?? [];
  const rows = transformedData.items.map((x) => ({
    //TODO: Show multiple leave requests (duration)
    ...x.employee,
    ...x.table.reduce(
      (a, v) => ({
        ...a,
        [v.date.toISO()!]: mapDuration(
          v.leaveRequests.find(() => true)
        ),
      }),
      {}
    ),
  }));

  const columns: GridColDef<(typeof rows)[number]>[] = [
    {
      field: "name",
      headerName: "",
    },
    ...dates.map((x) => ({
      field: x.toISO()!,
      headerName: x.toFormat("dd"),
      width: 10,
    })),
  ];

  const groups: GridColumnGroupingModel = [
    {
      groupId: "name",
      headerName: "",
      children: [{ field: 'name' }],
    },
    ...transformedData.header.map(x => (
      {
        groupId: x.date.toFormat("LLLL"),
        children: x.days.map(x => ({ field: x.toISO()! }))
      }

    ))
  ];
  console.log(groups);

  return (
    <Box sx={{maxWidth: '100%', overflow: 'auto'}}>
      <DataGrid
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
      />
    </Box>
  );
}

function mapDuration(LeaveRequest?: LeaveRequestDto): string {
  if (!LeaveRequest) {
    return "";
  }
  const duration = Duration.fromISO(LeaveRequest.duration);
  if (!duration.isValid) {
    //TODO: log invalid date
    return "";
  }
  const dateFrom = DateTime.fromISO(LeaveRequest.dateFrom);
  const dateTo = DateTime.fromISO(LeaveRequest.dateTo);
  const diff = dateTo.plus({day: 1}).diff(dateFrom, ["days"]);
  // https://github.com/moment/luxon/issues/422
  const durationPerDay = Duration.fromObject({ days: 0, hours: 0, seconds: 0, milliseconds: (duration.as('milliseconds') / diff.days)}).normalize();
  const timeResult = [];
  if (durationPerDay.days !== 0) {
    timeResult.push(`${durationPerDay.days}d`);
  }
  if (durationPerDay.hours !== 0) {
    timeResult.push(`${durationPerDay.hours}h`);
  }
  if (durationPerDay.minutes !== 0) {
    timeResult.push(`${durationPerDay.minutes}m`);
  }
  return timeResult.join(" ");
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

  return dateSequence.map(currentDate => ({
    date: currentDate,
    leaveRequests: leaveRequests.filter(
      (lr) => lr.dateFrom <= currentDate && lr.dateTo >= currentDate
    )
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
    days: createDatesSequence(dateFrom, dateFrom.plus({days: daysLeftDateFrom}))
  });
  for (
    let currentDate = DateTime.fromObject({year: dateFrom.year, month: dateFrom.month, day: 1}).plus({ month: 1 });
    currentDate.month < dateTo.month;
    currentDate = currentDate.plus({ month: 1 })
  ) {
    const daysLeft = getDaysInMonth(currentDate.year, currentDate.month) - 1;
    headerTable.push({
      date: currentDate,
      days: createDatesSequence(currentDate, currentDate.plus({days: daysLeft})),
    });
  }
  if (dateFrom.month != dateTo.month) {
    headerTable.push({
      date: dateTo,
      days: createDatesSequence(DateTime.fromObject({year: dateTo.year, month: dateTo.month, day: 1}), dateTo),
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

type LeaveRequest = LeaveRequestDto & {
  dateFrom: DateTime;
  dateTo: DateTime;
};

interface Employee {
  name: string;
  id: string;
}

interface HeaderTable {
  days: DateTime[];
  date: DateTime;
}
