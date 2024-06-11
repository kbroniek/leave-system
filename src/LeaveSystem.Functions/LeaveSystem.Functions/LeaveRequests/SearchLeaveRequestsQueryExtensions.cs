namespace LeaveSystem.Functions.LeaveRequests;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Http;

public static class SearchLeaveRequestsQueryExtensions
{
    public static SearchLeaveRequestsQueryDto Bind(this HttpContext context)
        => new(
            PageNumber: context.Request.Query.TryParseInt(nameof(SearchLeaveRequestsQueryDto.PageNumber)),
            PageSize: context.Request.Query.TryParseInt(nameof(SearchLeaveRequestsQueryDto.PageSize)),
            DateFrom: context.Request.Query.TryParseDateOnly(nameof(SearchLeaveRequestsQueryDto.DateFrom)),
            DateTo: context.Request.Query.TryParseDateOnly(nameof(SearchLeaveRequestsQueryDto.DateTo)),
            LeaveTypeIds: context.Request.Query.TryParseGuids(nameof(SearchLeaveRequestsQueryDto.LeaveTypeIds)),
            Statuses: context.Request.Query.TryParseLeaveRequestStatuses(nameof(SearchLeaveRequestsQueryDto.Statuses)),
            CreatedByEmails: context.Request.Query.TryParseStrings(nameof(SearchLeaveRequestsQueryDto.CreatedByEmails)),
            CreatedByUserIds: context.Request.Query.TryParseStrings(nameof(SearchLeaveRequestsQueryDto.CreatedByUserIds))
         );
}
