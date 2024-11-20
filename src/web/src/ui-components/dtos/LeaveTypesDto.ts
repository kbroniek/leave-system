export interface LeaveTypesDto {
    items: LeaveTypeDto[]
}
export interface LeaveTypeDto {
    id: string,
    name: string,
    properties?: {
        color: string,
        includeFreeDays: boolean,
        catalog: "Holiday" | "OnDemand" | "Sick" | "Saturday" | null
    },
    state: "Active" | "Inactive"
}