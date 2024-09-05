namespace LeaveSystem.Functions.LeaveRequests;

using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Http;

public static class SearchLeaveRequestsQueryExtensions
{
    public static SearchLeaveRequestsQueryDto BindSearchLeaveRequests(this HttpContext context)
        => new(
            ContinuationToken: context.Request.Query.TryParseString(nameof(SearchLeaveRequestsQueryDto.ContinuationToken)),
            DateFrom: context.Request.Query.TryParseDateOnly(nameof(SearchLeaveRequestsQueryDto.DateFrom)),
            DateTo: context.Request.Query.TryParseDateOnly(nameof(SearchLeaveRequestsQueryDto.DateTo)),
            LeaveTypeIds: context.Request.Query.TryParseGuids(nameof(SearchLeaveRequestsQueryDto.LeaveTypeIds)),
            Statuses: context.Request.Query.TryParseLeaveRequestStatuses(nameof(SearchLeaveRequestsQueryDto.Statuses)),
            AssignedToUserIds: context.Request.Query.TryParseStrings(nameof(SearchLeaveRequestsQueryDto.AssignedToUserIds))
         );
}
