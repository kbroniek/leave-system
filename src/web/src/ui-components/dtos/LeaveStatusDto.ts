export interface LeaveStatusesDto {
    items: LeaveStatusDto[]
}
export interface LeaveStatusDto {
    id: string,
    leaveRequestStatus: string,
    color: string,
    state: "Active" | "Inactive"
}