import { LeaveRequestsResponseDto } from "../dtos/LeaveRequestsDto";
import { DateTime } from "luxon";
import { alpha, styled } from "@mui/material/styles";
import Box from "@mui/material/Box";
import Table from "@mui/material/Table";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableBody from "@mui/material/TableBody";
import TableRow from "@mui/material/TableRow";
import TableCell from "@mui/material/TableCell";
import { RenderLeaveRequests } from "./RenderLeaveRequests";
import { HolidaysDto } from "../dtos/HolidaysDto";
import { LeaveStatusDto } from "../dtos/LeaveStatusDto";
import { RenderLeaveRequestModel } from "./RenderLeaveRequestModel";
import { LeaveTypeDto } from "../dtos/LeaveTypesDto";
import { EmployeeDto } from "../dtos/EmployeeDto";
import { LeaveRequestsTimelineTransformer } from "./LeaveRequestsTimelineTransformer";
import { EmployeesFinder } from "../utils/EmployeesFinder";
import { SubmitLeaveRequestDialog } from "../submit-leave-request/SubmitLeaveRequestDialog";
import { LeaveRequestFormModel } from "../submit-leave-request/SubmitLeaveRequestForm";
import { LeaveLimitDto } from "../dtos/LeaveLimitsDto";
import { useState } from "react";
import { SubmitHandler } from "react-hook-form";
import { useTranslation } from "react-i18next";

export const rowHeight = 30;

function getMonthTranslationKey(monthNumber: number): string {
  const monthKeys = [
    "january",
    "february",
    "march",
    "april",
    "may",
    "june",
    "july",
    "august",
    "september",
    "october",
    "november",
    "december",
  ];
  return monthKeys[monthNumber - 1] || "january";
}

export default function ShowLeaveRequestsTimeline(
  params: Readonly<{
    leaveRequests: LeaveRequestsResponseDto;
    holidays: HolidaysDto;
    leaveStatuses: LeaveStatusDto[];
    leaveTypes: LeaveTypeDto[];
    employees: EmployeeDto[];
    leaveLimits?: LeaveLimitDto[];
    onSubmitLeaveRequest?: SubmitHandler<LeaveRequestFormModel>;
    onYearChanged?: (year: string) => void;
    onUserIdChanged?: (userId: string) => void;
  }>
): React.ReactElement {
  const { t } = useTranslation();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedDate, setSelectedDate] = useState<DateTime | undefined>();
  const [selectedEmployee, setSelectedEmployee] = useState<
    EmployeeDto | undefined
  >();

  const handleAddLeaveRequest = (date: DateTime, employee: EmployeeDto) => {
    setSelectedDate(date);
    setSelectedEmployee(employee);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedDate(undefined);
    setSelectedEmployee(undefined);
  };

  const employees = EmployeesFinder.get(
    params.leaveRequests?.items || [],
    params.employees || []
  );
  const leaveStatusesActive = params.leaveStatuses || [];
  const leaveTypesActive = (params.leaveTypes || []).filter(
    (x) => x.state === "Active"
  );
  const transformer = new LeaveRequestsTimelineTransformer(
    employees,
    params.leaveRequests,
    params.holidays || { items: [] },
    params.leaveTypes || []
  );
  const transformedData = transformer.transformToTable();
  const transformedHolidays = (params.holidays?.items || []).map((x) =>
    DateTime.fromISO(x)
  );
  const ODD_OPACITY = 0.2;
  const StyledTableContainer = styled(TableContainer)(({ theme }) => ({
    maxHeight: "100%",
    overflow: "auto",
    "& .MuiTableCell-root": {
      padding: 0,
      height: rowHeight,
    },
    "& .MuiTableCell-head": {
      position: "sticky",
      top: 0,
      zIndex: 3,
      backgroundColor: theme.palette.background.paper,
      borderBottom: `1px solid ${theme.palette.divider}`,
    },
    "& .employee-name-cell": {
      position: "sticky",
      left: 0,
      zIndex: 401,
      paddingLeft: "8px",
      paddingRight: "8px",
      borderRight: `1px solid ${theme.palette.divider}`,
      backgroundColor: theme.palette.background.paper,
      fontWeight: 500,
      display: "flex",
      alignItems: "center",
    },
    "& .employee-name-header": {
      position: "sticky",
      left: 0,
      top: 0,
      zIndex: 4,
      backgroundColor: theme.palette.grey[100],
      borderRight: `1px solid ${theme.palette.divider}`,
      fontWeight: 600,
      paddingLeft: "8px",
      paddingRight: "8px",
    },
    "& .timeline-day.date-from": {
      color: "#ff0000",
    },
    "& .timeline-day.date-to": {
      color: "#ff00ff",
    },
    "& .timeline-day.today": {
      backgroundColor: "#c0c0c0",
    },
    "& .timeline-day.weekend": {
      backgroundColor: "#e0e006",
    },
    "& .timeline-day.holiday": {
      backgroundColor: "#FFDD96",
    },
    "& .month-separator": {
      borderRight: "1px solid rgba(0, 0, 0, 0.5)",
    },
  }));

  const StyledTable = styled(Table)(({ theme }) => ({
    "& .MuiTableRow-root": {
      "&:hover": {
        backgroundColor: alpha(theme.palette.primary.main, ODD_OPACITY),
      },
    },
    "& .MuiTableRow-root.odd": {
      "& .timeline-day.weekend": {
        backgroundColor: "#b8b82e",
        "&:hover": {
          backgroundColor: alpha("#b8b82e", ODD_OPACITY),
        },
      },
      "& .timeline-day.holiday": {
        backgroundColor: "#d4b97c",
        "&:hover": {
          backgroundColor: alpha("#d4b97c", ODD_OPACITY),
        },
      },
      "& .employee-name-cell": {
        backgroundColor: theme.palette.grey[100],
        "&:hover": {
          backgroundColor: alpha(theme.palette.grey[100], ODD_OPACITY),
        },
      },
      backgroundColor: theme.palette.grey[200],
      "&:hover": {
        backgroundColor: alpha(theme.palette.primary.main, ODD_OPACITY),
      },
    },
  }));

  return (
    <Box sx={{ maxWidth: "100%", overflow: "auto", flexGrow: 1 }}>
      <StyledTableContainer>
        <StyledTable stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell
                className="employee-name-header"
                sx={{ minWidth: 200, width: 200 }}
              >
                {/* Empty header for employee name column */}
              </TableCell>
              {transformedData.header.map((monthGroup, monthIndex) => (
                <TableCell
                  key={monthGroup.date.toISO()}
                  colSpan={monthGroup.days.length}
                  className={
                    monthIndex < transformedData.header.length - 1
                      ? "month-separator"
                      : ""
                  }
                  sx={{
                    textAlign: "center",
                    fontWeight: "bold",
                    borderLeft: "1px solid",
                    borderColor: "divider",
                  }}
                >
                  {(() => {
                    const monthKey = getMonthTranslationKey(
                      monthGroup.date.month
                    );
                    const translationKey = `months.${monthKey}`;
                    const translated = t(translationKey);
                    // If translation returns the key itself (translation not found), use Luxon format
                    if (
                      translated === translationKey ||
                      translated.startsWith("months.")
                    ) {
                      return monthGroup.date.toFormat("LLLL");
                    }
                    return translated;
                  })()}
                </TableCell>
              ))}
            </TableRow>
            <TableRow>
              <TableCell
                className="employee-name-header"
                sx={{ minWidth: 200, width: 200 }}
              >
                {/* Empty header for employee name column */}
              </TableCell>
              {transformedData.header.map((monthGroup, monthIndex) =>
                monthGroup.days.map((day, dayIndex) => {
                  const isLastDayOfMonth =
                    dayIndex === monthGroup.days.length - 1;
                  const isLastMonth =
                    monthIndex === transformedData.header.length - 1;
                  return (
                    <TableCell
                      key={day.toISO()}
                      className={`${getDayCssClass(day, transformedHolidays)} ${
                        isLastDayOfMonth && !isLastMonth
                          ? "month-separator"
                          : ""
                      }`}
                      sx={{
                        textAlign: "center",
                        borderLeft: "1px solid",
                        borderColor: "divider",
                      }}
                    >
                      {day.toFormat("dd")}
                    </TableCell>
                  );
                })
              )}
            </TableRow>
          </TableHead>
          <TableBody>
            {transformedData.items.map((item, rowIndex) => (
              <TableRow
                key={item.employee.id}
                className={rowIndex % 2 === 0 ? "even" : "odd"}
              >
                <TableCell className="employee-name-cell">
                  {item.employee.name}
                </TableCell>
                {transformedData.header.map((monthGroup, monthIndex) =>
                  monthGroup.days.map((day, dayIndex) => {
                    const dayData = item.table.find((d) => d.date.equals(day));
                    const isLastDayOfMonth =
                      dayIndex === monthGroup.days.length - 1;
                    const isLastMonth =
                      monthIndex === transformedData.header.length - 1;

                    if (!dayData) {
                      return (
                        <TableCell
                          key={day.toISO()}
                          className={`${getDayCssClass(
                            day,
                            transformedHolidays
                          )} ${
                            isLastDayOfMonth && !isLastMonth
                              ? "month-separator"
                              : ""
                          }`}
                          sx={{
                            borderLeft: "1px solid",
                            borderColor: "divider",
                          }}
                        />
                      );
                    }

                    const renderModel: RenderLeaveRequestModel = {
                      date: dayData.date,
                      leaveRequests: dayData.leaveRequests,
                      statuses: leaveStatusesActive,
                      leaveTypes: leaveTypesActive,
                      holidays: params.holidays || { items: [] },
                    };
                    return (
                      <TableCell
                        key={dayData.date.toISO()}
                        className={`${getDayCssClass(
                          dayData.date,
                          transformedHolidays
                        )} ${
                          isLastDayOfMonth && !isLastMonth
                            ? "month-separator"
                            : ""
                        }`}
                        sx={{
                          borderLeft: "1px solid",
                          borderColor: "divider",
                        }}
                      >
                        <RenderLeaveRequests
                          value={renderModel}
                          row={item.employee}
                          onAddLeaveRequest={handleAddLeaveRequest}
                        />
                      </TableCell>
                    );
                  })
                )}
              </TableRow>
            ))}
          </TableBody>
        </StyledTable>
      </StyledTableContainer>

      <SubmitLeaveRequestDialog
        open={dialogOpen}
        onClose={handleCloseDialog}
        selectedDate={selectedDate}
        selectedEmployee={selectedEmployee}
        leaveRequests={params.leaveRequests?.items}
        holidays={params.holidays}
        leaveTypes={leaveTypesActive}
        leaveLimits={params.leaveLimits}
        employees={params.employees}
        onSubmit={
          params.onSubmitLeaveRequest || (() => Promise.resolve(undefined))
        }
        onYearChanged={params.onYearChanged || (() => {})}
        onUserIdChanged={params.onUserIdChanged || (() => {})}
      />
    </Box>
  );
}

function getDayCssClass(date: DateTime, holidays: DateTime[]): string {
  if (DateTime.local().startOf("day").equals(date)) {
    return "timeline-day today";
  }
  if (holidays.find((x) => x.equals(date))) {
    return "timeline-day holiday";
  }
  return date.isWeekend ? "timeline-day weekend" : "timeline-day";
}
