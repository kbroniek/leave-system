import { LeaveRequestDto } from "../dtos/LeaveRequestsDto";
import { UserDto } from "../dtos/UserDto";

export class EmployeesFinder {
  public static get(
    leaveRequests: Readonly<LeaveRequestDto[]> | undefined,
    employees: Readonly<UserDto[]> | undefined,
  ): UserDto[] {
    const allEmployees = employees
      ? employees.map((x) => ({
          ...x,
          name: x.lastName
            ? `${x.lastName} ${x.firstName}`
            : (x.name ?? "undefined"),
        }))
      : [];
    if (!leaveRequests) {
      return allEmployees;
    }
    for (const e of leaveRequests) {
      if (!allEmployees.find((x) => x.id === e.assignedTo.id)) {
        allEmployees.push({
          ...e.assignedTo,
          name: e.assignedTo.name ?? "undefined",
        });
      }
    }
    const employeesSorted = allEmployees.sort(
      (a, b) => a.name?.localeCompare(b.name ?? "") ?? 0,
    );
    return employeesSorted;
  }
}
