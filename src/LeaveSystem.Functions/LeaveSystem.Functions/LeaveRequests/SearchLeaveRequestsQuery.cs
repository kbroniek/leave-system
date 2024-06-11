using LeaveSystem.Shared.LeaveRequests;
using Microsoft.AspNetCore.Http;

namespace LeaveSystem.Functions.LeaveRequests;

public record SearchLeaveRequestsQuery(int? PageNumber, int? PageSize, DateOnly? DateFrom, DateOnly? DateTo,
    Guid[]? LeaveTypeIds, LeaveRequestStatus[]? Statuses, string[]? CreatedByEmails, string[]? CreatedByUserIds)
{
    public static SearchLeaveRequestsQuery Bind(HttpContext context)
        => new(
            PageNumber: context.Request.Query.TryParseInt(nameof(PageNumber)),
            PageSize: context.Request.Query.TryParseInt(nameof(PageSize)),
            DateFrom: context.Request.Query.TryParseDateOnly(nameof(DateFrom)),
            DateTo: context.Request.Query.TryParseDateOnly(nameof(DateTo)),
            LeaveTypeIds: context.Request.Query.TryParseGuids(nameof(LeaveTypeIds)),
            Statuses: context.Request.Query.TryParseLeaveRequestStatuses(nameof(Statuses)),
            CreatedByEmails: context.Request.Query.TryParseStrings(nameof(CreatedByEmails)),
            CreatedByUserIds: context.Request.Query.TryParseStrings(nameof(CreatedByUserIds))
         );
}
