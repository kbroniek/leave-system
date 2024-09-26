import { LeaveRequestDto, LeaveRequestsResponseDto } from './LeaveRequestsDto';
import { DateTime } from 'luxon';
import Box from '@mui/material/Box';
import { DataGrid } from '@mui/x-data-grid/DataGrid';
import { GridColDef } from '@mui/x-data-grid/models';

export default function ShowLeaveRequestsTimeline(apiData: LeaveRequestsResponseDto) {
  // TODO: Get employee from api
  const employees: Employee[] = [...new Map(apiData.items.map(item =>
    [item.createdBy.id, item.createdBy])).values()];
  const transformedData = transformToTable(apiData, employees);
  const dates = transformedData.items.find(() => true)?.table.map(x => x.date) ?? [];
  const rows = transformedData.items.map(x => ({
    //TODO: Show multiple leave requests (duration)
    ...x.employee,
    ...x.table.reduce((a, v) => ({ ...a, [v.date.toISO()!]: v.leaveRequests.find(() => true)?.duration}), {})
  }));

  const columns: GridColDef<(typeof rows)[number]>[] = [
    {

      field: "name",
      headerName: ""
    },
    ...dates.map(x => ({
    field: x.toISO()!,
    headerName: x.toFormat('dd'),
    width: 10,
  }))];

  return (
    <Box sx={{ height: 400, width: '100%' }}>
      <DataGrid
        rows={rows}
        columns={columns}
        disableRowSelectionOnClick
        hideFooter={true}
        hideFooterPagination={true}
        hideFooterSelectedRowCount={true}
        disableColumnMenu
        disableColumnSorting
        disableColumnResize
      />
    </Box>
  )
}

function buildDateTime(leaveRequests: LeaveRequestDto[]): LeaveRequest[] {
  return leaveRequests.map(x => ({
    ...x,
    dateFrom: DateTime.fromISO(x.dateFrom),
    dateTo: DateTime.fromISO(x.dateTo)
    //TODO parse duration
  }) as LeaveRequest);
}

function transformToTable(leaveRequestsResponse: LeaveRequestsResponseDto, employees: Employee[]): UserLeaveRequestTableCollection {
  const dateFrom = DateTime.fromISO(leaveRequestsResponse.search.dateFrom);
  const dateTo = DateTime.fromISO(leaveRequestsResponse.search.dateTo);
  // Ensure startDay is before endDay
  if (dateFrom > dateTo) {
    throw new Error("endDay must be after startDay");
  }

  const leaveRequests = buildDateTime(leaveRequestsResponse.items);
  return {
    items: employees.map(x => ({
      employee:
      {
        id: x.id,
        name: x.name,
      },
      table: transformLeaveRequest(leaveRequests.filter(lr => lr.createdBy.id === x.id), dateFrom, dateTo),
    })),
    header: transformHeader(dateFrom, dateTo)
  };
}

function transformLeaveRequest(leaveRequests: LeaveRequest[], dateFrom: DateTime, dateTo: DateTime): LeaveRequestTable[] {
  const table: LeaveRequestTable[] = [];
  let currentDate = dateFrom;
  // Max one year
  for (let i = 0; i < 365 && currentDate <= dateTo; ++i, currentDate = currentDate.plus({ days: 1 })) {
    const filteredLeaveRequests = leaveRequests.filter(x => x.dateFrom <= currentDate && x.dateTo >= currentDate);
    table.push({
      date: currentDate,
      leaveRequests: filteredLeaveRequests
    });
  }
  return table;
}
function transformHeader(dateFrom: DateTime, dateTo: DateTime): HeaderTable[] {
  const headerTable: HeaderTable[] = [];
  const daysLeftDateFrom = getDaysInMonth(dateFrom.year, dateFrom.month) - dateFrom.day + 1;
  headerTable.push({
    date: dateFrom,
    daysLeft: daysLeftDateFrom
  });
  for (let currentDate = dateFrom.plus({ month: 1 }); currentDate.month < dateTo.month; currentDate = currentDate.plus({month: 1})) {
    const daysLeft = getDaysInMonth(currentDate.year, currentDate.month);
    headerTable.push({
      date: currentDate,
      daysLeft
    });
  }
  if (dateFrom.month != dateTo.month) {
    headerTable.push({
      date: dateTo,
      daysLeft: dateTo.day
    });
  }
  return headerTable
}

function getDaysInMonth(year: number, month: number): number {
  return DateTime.fromObject({ year, month }).daysInMonth!;
}
interface UserLeaveRequestTableCollection {
  items: UserLeaveRequestTable[]
  header: HeaderTable[]
}
interface UserLeaveRequestTable {
  employee: {
    name: string
    id: string
  }
  table: LeaveRequestTable[]
}
interface LeaveRequestTable {
  date: DateTime
  leaveRequests: LeaveRequest[]
}

type LeaveRequest = LeaveRequestDto & {
  dateFrom: DateTime
  dateTo: DateTime
}

interface Employee {
  name: string,
  id: string
}

interface HeaderTable {
  daysLeft: number,
  date: DateTime
}