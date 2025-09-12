import { TFunction } from "i18next";
import { ErrorCodes, ApiError } from "../types";

// Error code to user-friendly message mapping
export const getErrorMessage = (error: ApiError, t: TFunction): string => {
  const { errorCode, detail, title } = error;

  // Map error codes to translated messages from backend
  const errorMessages: Record<string, string> = {
    // Validation errors
    [ErrorCodes.INVALID_INPUT]: t("errors.invalidInput", { detail }),
    [ErrorCodes.INVALID_DATE_RANGE]: t("errors.invalidDateRange"),
    [ErrorCodes.OFF_WORK_DATE]: t("errors.offWorkDate"),

    // Resource errors
    [ErrorCodes.RESOURCE_CONFLICT]: t("errors.resourceConflict"),
    [ErrorCodes.RESOURCE_NOT_FOUND]: t("errors.resourceNotFound"),
    [ErrorCodes.RESOURCE_EXISTS]: t("errors.resourceExists"),
    [ErrorCodes.LEAVE_TYPE_NOT_FOUND]: t("errors.leaveTypeNotFound"),
    [ErrorCodes.GRAPH_USER_NOT_FOUND]: t("errors.userNotFound"),
    [ErrorCodes.UNEXPECTED_GRAPH_ERROR]: t("errors.unexpectedGraphError"),
    [ErrorCodes.UNEXPECTED_DB_ERROR]: t("errors.unexpectedDatabaseError"),

    // Business logic errors
    [ErrorCodes.INSUFFICIENT_LEAVE_DAYS]: t("errors.insufficientLeaveDays"),
    [ErrorCodes.OVERLAPPING_LEAVE]: t("errors.overlappingLeave"),
    [ErrorCodes.INVALID_STATUS_TRANSITION]: t("errors.invalidStatusTransition"),
    [ErrorCodes.PAST_LEAVE_MODIFICATION]: t("errors.pastLeaveModification"),

    // Authorization errors
    [ErrorCodes.FORBIDDEN_OPERATION]: t("errors.forbiddenOperation"),
    [ErrorCodes.EMPLOYEE_NOT_FOUND]: t("errors.employeeNotFound"),
    [ErrorCodes.UNAUTHORIZED_ACCESS]: t("errors.unauthorizedAccess"),
    [ErrorCodes.ROLES_NOT_FOUND]: t("errors.rolesNotFound"),

    // Data validation and configuration errors
    [ErrorCodes.LIMITS_NOT_CONFIGURED]: t("errors.limitsNotConfigured"),
    [ErrorCodes.DUPLICATE_LIMITS]: t("errors.duplicateLimits"),
    [ErrorCodes.DUPLICATE_ROLES]: t("errors.duplicateRoles"),
    [ErrorCodes.INVALID_COLOR_FORMAT]: t("errors.invalidColorFormat"),
    [ErrorCodes.INVALID_ID_MISMATCH]: t("errors.invalidIdMismatch"),
    [ErrorCodes.INVALID_ROLE]: t("errors.invalidRole"),
    [ErrorCodes.QUERY_PARAMETER_ERROR]: t("errors.queryParameterError"),
  };

  // Return the mapped message or fall back to the original title/detail
  return (
    errorMessages[errorCode] || title || detail || t("errors.unknownError")
  );
};

// Determine error severity based on error code
export const getErrorSeverity = (
  errorCode: string,
): "error" | "warning" | "info" => {
  const warningSeverityCodes = [
    ErrorCodes.INVALID_INPUT,
    ErrorCodes.INVALID_DATE_RANGE,
    ErrorCodes.OFF_WORK_DATE,
    ErrorCodes.OVERLAPPING_LEAVE,
    ErrorCodes.INSUFFICIENT_LEAVE_DAYS,
  ];

  const infoSeverityCodes = [
    ErrorCodes.RESOURCE_EXISTS,
    ErrorCodes.INVALID_STATUS_TRANSITION,
  ];

  if (warningSeverityCodes.includes(errorCode as ErrorCodes)) {
    return "warning";
  }

  if (infoSeverityCodes.includes(errorCode as ErrorCodes)) {
    return "info";
  }

  return "error";
};

// Get auto-hide duration based on error severity
export const getAutoHideDuration = (errorCode: string): number => {
  const severity = getErrorSeverity(errorCode);

  switch (severity) {
    case "info":
      return 4000;
    case "warning":
      return 6000;
    case "error":
      return 8000;
    default:
      return 5000;
  }
};
