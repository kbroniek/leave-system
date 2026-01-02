namespace LeaveSystem.Domain;

using System.Net;

public record struct Error(string Message, HttpStatusCode HttpStatusCode, string ErrorCode)
{
    public static Error Conflict(string resource, string errorCode = ErrorCodes.RESOURCE_CONFLICT) =>
        new($"The request could not be completed due to a conflict with the current state of the resource. {resource} Please resolve the conflict and try again.", HttpStatusCode.Conflict, errorCode);
    public static Error BadRequest(string parameter, string errorCode = ErrorCodes.INVALID_INPUT) =>
        new($"Missing required fields or invalid input format. Please verify the request payload and parameters. [{parameter}]", HttpStatusCode.BadRequest, errorCode);
}

public static class ErrorCodes
{
    // Validation errors
    public const string INVALID_INPUT = "INVALID_INPUT";
    public const string INVALID_DATE_RANGE = "INVALID_DATE_RANGE";
    public const string OFF_WORK_DATE = "OFF_WORK_DATE";

    // Resource errors
    public const string RESOURCE_CONFLICT = "RESOURCE_CONFLICT";
    public const string RESOURCE_NOT_FOUND = "RESOURCE_NOT_FOUND";
    public const string RESOURCE_EXISTS = "RESOURCE_EXISTS";
    public const string LEAVE_TYPE_NOT_FOUND = "LEAVE_TYPE_NOT_FOUND";
    public const string GRAPH_USER_NOT_FOUND = "GRAPH_UNEXPECTED_ERROR";
    public const string UNEXPECTED_GRAPH_ERROR = "UNEXPECTED_GRAPH_ERROR";
    public const string UNEXPECTED_DB_ERROR = "UNEXPECTED_DB_ERROR";

    // Business logic errors
    public const string INSUFFICIENT_LEAVE_DAYS = "INSUFFICIENT_LEAVE_DAYS";
    public const string OVERLAPPING_LEAVE = "OVERLAPPING_LEAVE";
    public const string INVALID_STATUS_TRANSITION = "INVALID_STATUS_TRANSITION";
    public const string PAST_LEAVE_MODIFICATION = "PAST_LEAVE_MODIFICATION";

    // Authorization errors
    public const string FORBIDDEN_OPERATION = "FORBIDDEN_OPERATION";
    public const string EMPLOYEE_NOT_FOUND = "EMPLOYEE_NOT_FOUND";
    public const string UNAUTHORIZED_ACCESS = "UNAUTHORIZED_ACCESS";
    public const string ROLES_NOT_FOUND = "ROLES_NOT_FOUND";

    // Data validation and configuration errors
    public const string LIMITS_NOT_CONFIGURED = "LIMITS_NOT_CONFIGURED";
    public const string DUPLICATE_LIMITS = "DUPLICATE_LIMITS";
    public const string DUPLICATE_ROLES = "DUPLICATE_ROLES";
    public const string INVALID_COLOR_FORMAT = "INVALID_COLOR_FORMAT";
    public const string INVALID_ID_MISMATCH = "INVALID_ID_MISMATCH";
    public const string INVALID_ROLE = "INVALID_ROLE";
    public const string QUERY_PARAMETER_ERROR = "QUERY_PARAMETER_ERROR";

    public const string EMPLOYEE_ACCOUNT_DISABLED = "EMPLOYEE_ACCOUNT_DISABLED";
}
