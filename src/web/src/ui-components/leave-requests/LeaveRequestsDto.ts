import { LeaveRequestStatus } from "../dtos/LeaveRequestStatusDto"
import { UserDto } from "../dtos/UserDto"

export interface LeaveRequestsResponseDto {
    items: LeaveRequestDto[],
    continuationToken: string | null,
    search: {
        dateFrom: string,
        dateTo: string,
        leaveTypeIds: string[] | null,
        statuses: ["Init", "Pending", "Accepted", "Canceled", "Rejected", "Deprecated"],
        assignedToUserIds: string[] | null
    }
}

export interface LeaveRequestDto {
    id: string
    dateFrom: string
    dateTo: string
    duration: string
    leaveTypeId: string
    status: LeaveRequestStatus
    createdBy: UserDto
    workingHours: string
}
