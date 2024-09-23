import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import { LeaveRequestDto, LeaveRequestsResponseDto } from './LeaveRequestsDto';
import { DateTime, DateTimeMaybeValid } from 'luxon';


export default function ShowLeaveRequestsTimeline(apiData: LeaveRequestsResponseDto) {
   // TODO: Get from api
  const employees: Employee[] = [...new Map(apiData.items.map(item =>
    [item.createdBy.id, item.createdBy])).values()];
  const transformedData = transformToTable(apiData, employees);
  const dates = transformedData.find(() => true)?.table.map(x => x.date);
  return (
    <TableContainer component={Paper}>
      <Table sx={{ minWidth: 650 }} size="small" aria-label="a dense table">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            {dates?.map(date => (
              <TableCell align="right">{date.toISODate()}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {transformedData.map((row) => (
            <TableRow
              key={row.username}
              sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
            >
              <TableCell component="th" scope="row">
                {row.username}
              </TableCell>
              {row.table.map(data => (
                <TableCell align="right">{data.leaveRequests.find(() => true)?.duration}</TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

interface UserLeaveRequestTable {
  username: string
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

function buildDateTime(leaveRequests: LeaveRequestDto[]) : LeaveRequest[] {
    return leaveRequests.map(x => ({
      ...x,
      dateFrom: DateTime.fromISO(x.dateFrom),
      dateTo: DateTime.fromISO(x.dateTo)
      //TODO parse duration
    }) as LeaveRequest);
}

interface Employee {
  name: string,
  id: string
}

function transformToTable(leaveRequestsResponse: LeaveRequestsResponseDto, employees: Employee[]) : UserLeaveRequestTable[] {

  const dateFrom = DateTime.fromISO(leaveRequestsResponse.search.dateFrom);
  const dateTo = DateTime.fromISO(leaveRequestsResponse.search.dateTo);

  const leaveRequests = buildDateTime(leaveRequestsResponse.items);
  return employees.map(x => ({
    username: x.name,
    table: transformLeaveRequest(leaveRequests.filter(lr => lr.createdBy.id === x.id), dateFrom, dateTo)
  }));
}

function transformLeaveRequest(leaveRequests: LeaveRequest[], dateFrom: DateTime, dateTo: DateTime): LeaveRequestTable[] {
  const table: LeaveRequestTable[] = [];
  let currentDate = dateFrom;
  // Max one year
  for(let i = 0; i < 365 && currentDate <= dateTo; ++i, currentDate = currentDate.plus({ days: 1 })) {
    const filteredLeaveRequests = leaveRequests.filter(x => x.dateFrom <= currentDate && x.dateTo >= currentDate);
    table.push({
      date: currentDate,
      leaveRequests: filteredLeaveRequests
    });
  }
  return table;
}
