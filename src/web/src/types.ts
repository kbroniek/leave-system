import type { IPublicClientApplication } from "@azure/msal-browser";

export type AppProps = {
  pca: IPublicClientApplication;
};

export type GraphData = {
  displayName: string;
  jobTitle: string;
  mail: string;
  businessPhones: string[];
  officeLocation: string;
};

// Error handling types
export interface ApiError {
  type?: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errorCode: string;
}

export interface ApiErrorResponse {
  error: ApiError;
}

// Error codes enum matching backend
export enum ErrorCodes {
  // Validation errors
  INVALID_INPUT = "INVALID_INPUT",
  INVALID_DATE_RANGE = "INVALID_DATE_RANGE",
  OFF_WORK_DATE = "OFF_WORK_DATE",

  // Resource errors
  RESOURCE_CONFLICT = "RESOURCE_CONFLICT",
  RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND",
  RESOURCE_EXISTS = "RESOURCE_EXISTS",
  LEAVE_TYPE_NOT_FOUND = "LEAVE_TYPE_NOT_FOUND",
  GRAPH_USER_NOT_FOUND = "GRAPH_USER_NOT_FOUND",
  UNEXPECTED_GRAPH_ERROR = "UNEXPECTED_GRAPH_ERROR",
  UNEXPECTED_DB_ERROR = "UNEXPECTED_DB_ERROR",

  // Business logic errors
  INSUFFICIENT_LEAVE_DAYS = "INSUFFICIENT_LEAVE_DAYS",
  OVERLAPPING_LEAVE = "OVERLAPPING_LEAVE",
  INVALID_STATUS_TRANSITION = "INVALID_STATUS_TRANSITION",
  PAST_LEAVE_MODIFICATION = "PAST_LEAVE_MODIFICATION",

  // Authorization errors
  FORBIDDEN_OPERATION = "FORBIDDEN_OPERATION",
  EMPLOYEE_NOT_FOUND = "EMPLOYEE_NOT_FOUND",
  UNAUTHORIZED_ACCESS = "UNAUTHORIZED_ACCESS",
  ROLES_NOT_FOUND = "ROLES_NOT_FOUND",

  // Data validation and configuration errors
  LIMITS_NOT_CONFIGURED = "LIMITS_NOT_CONFIGURED",
  DUPLICATE_LIMITS = "DUPLICATE_LIMITS",
  DUPLICATE_ROLES = "DUPLICATE_ROLES",
  INVALID_COLOR_FORMAT = "INVALID_COLOR_FORMAT",
  INVALID_ID_MISMATCH = "INVALID_ID_MISMATCH",
  INVALID_ROLE = "INVALID_ROLE",
  QUERY_PARAMETER_ERROR = "QUERY_PARAMETER_ERROR",
}
