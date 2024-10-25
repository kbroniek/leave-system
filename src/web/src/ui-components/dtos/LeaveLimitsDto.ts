export interface LeaveLimitsDto {
  items: LeaveLimitDto[];
}
export interface LeaveLimitDto {
  id: string;
  limit: string;
  overdueLimit: string | null;
  workingHours: string;
  leaveTypeId: string;
  validSince: string;
  validUntil: string;
  assignedToUserId: string;
  state: "Active" | "Inactive";
  description: string | null;
}
