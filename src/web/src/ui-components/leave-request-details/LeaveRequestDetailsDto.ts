import { LeaveRequestStatus } from "../dtos/LeaveRequestStatusDto"
import { UserDto } from "../dtos/UserDto"

export interface LeaveRequestDetailsDto {
    leaveRequestId: string
    dateFrom: string
    dateTo: string
    duration: string
    leaveTypeId: string
    status: LeaveRequestStatus
    createdBy: UserDto
    assignedTo: UserDto
    lastModifiedBy: UserDto
    workingHours: string
    createdDate: string
    lastModifiedDate: string
    remarks: RemarksDto[]
}

export interface RemarksDto {
    remarks: string
    createdBy: UserDto
    createdDate: string
    status: LeaveRequestStatus
}
