export interface LeaveTypesDto {
    items: LeaveTypeDto[]
}
export interface LeaveTypeDto {
    id: string,
    name: string,
    properties?: {
        color: string
        includeFreeDays: boolean
    },
    state: "Active" | "Inactive"
}