export interface LeaveLimitsDto {
  items: LeaveLimitDto[];
}
export interface LeaveLimitDto {
  id: string;
  limit: string | null;
  overdueLimit: string | null;
  workingHours: string;
  leaveTypeId: string;
  validSince: string | null;
  validUntil: string | null;
  assignedToUserId: string;
  state: "Active" | "Inactive";
  description: string | null;
}
