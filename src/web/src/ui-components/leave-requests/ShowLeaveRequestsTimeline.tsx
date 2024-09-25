import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import { LeaveRequestDto, LeaveRequestsResponseDto } from './LeaveRequestsDto';
import { DateTime, DateTimeMaybeValid } from 'luxon';
import { Box } from '@mui/material';


export default function ShowLeaveRequestsTimeline(apiData: LeaveRequestsResponseDto) {
  console.log(DateTime.fromISO("1.09:24:15,123"));
  // TODO: Get employee from api
  const employees: Employee[] = [...new Map(apiData.items.map(item =>
    [item.createdBy.id, item.createdBy])).values()];
  const transformedData = transformToTable(apiData, employees);
  const dates = transformedData.items.find(() => true)?.table.map(x => x.date);
  return (
    <TableContainer component={Paper} >
      <Table  aria-label="a dense table">
        <TableHead>
          {/* <TableRow>
            <TableCell key="header-empty"></TableCell>
            {transformedData.header?.map(item => (
            <TableCell sx={{ whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis", width: 10}} key={`header-month-${item.date.toISODate()}`} colSpan={item.daysLeft}><></>{item.date.toFormat('LLLL')}</TableCell>
            ))}
          </TableRow> */}
          <TableRow>
            <TableCell key="empty"></TableCell>
            {dates?.map(date => (
              <TableCell sx={{whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis", width: 10}} key={date.toISODate()} align="right">{date.toFormat('dd')}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {transformedData.items.map((row) => (
            <TableRow
              key={row.employee.id}
              sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
            >
              <TableCell component="th" scope="row">
                {row.employee.name}
              </TableCell>
              {row.table.map(data => (
                <TableCell  sx={{whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis", width: 40}} key={`${row.employee.id}/${data.date.toISODate()}`}  align="right">{data.leaveRequests.find(() => true)?.duration}</TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
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
  dateFrom: DateTimeMaybeValid
  dateTo: DateTimeMaybeValid
}

interface Employee {
  name: string,
  id: string
}

interface HeaderTable {
  daysLeft: number,
  date: DateTime
}