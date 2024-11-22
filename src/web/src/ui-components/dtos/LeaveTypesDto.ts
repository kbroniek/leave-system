export interface LeaveTypesDto {
    items: LeaveTypeDto[]
}
export type LeaveTypeCatalog = "Holiday" | "OnDemand" | "Sick" | "Saturday";
export interface LeaveTypeDto {
    id: string,
    name: string,
    properties?: {
        color: string,
        includeFreeDays: boolean,
        catalog: LeaveTypeCatalog | null
    },
    state: "Active" | "Inactive"
}