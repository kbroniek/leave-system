export interface LeaveTypesDto {
    items: LeaveTypeDto[]
}
export interface LeaveTypeDto {
    id: string,
    name: string,
    properties: {
        color: string
    },
    state: "Active" | "Inactive"
}