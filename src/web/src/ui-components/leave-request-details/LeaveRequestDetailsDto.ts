import { LeaveRequestStatus } from "../dtos/LeaveRequestStatusDto"
import { EmployeeDto } from "../dtos/EmployeeDto"

export interface LeaveRequestDetailsDto {
    leaveRequestId: string
    dateFrom: string
    dateTo: string
    duration: string
    leaveTypeId: string
    status: LeaveRequestStatus
    createdBy: EmployeeDto
    assignedTo: EmployeeDto
    lastModifiedBy: EmployeeDto
    workingHours: string
    createdDate: string
    lastModifiedDate: string
    remarks: RemarksDto[]
}

export interface RemarksDto {
    remarks: string
    createdBy: EmployeeDto
    createdDate: string
    status: LeaveRequestStatus
}
