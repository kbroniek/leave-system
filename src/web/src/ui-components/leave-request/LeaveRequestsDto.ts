export interface LeaveRequestsDto {
    items: [
        {
            id: string
            dateFrom: string,
            dateTo: string,
            duration: string,
            leaveTypeId: string,
            status: string,
            createdByName: string,
            workingHours: string
        }
    ]
  }