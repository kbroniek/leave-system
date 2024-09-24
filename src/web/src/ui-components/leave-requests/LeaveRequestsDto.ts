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
    status: string
    createdBy: {
        id:string
        name: string
    }
    workingHours: string
}
