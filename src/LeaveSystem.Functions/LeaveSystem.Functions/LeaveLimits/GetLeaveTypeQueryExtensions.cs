namespace LeaveSystem.Functions.LeaveLimits;

using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Http;

public static class GetLeaveTypeQueryExtensions
{
    public static GetLeaveLimitQuery BindGetLeaveLimitQuery(this HttpContext context)
        => new(
            DateFrom: context.Request.Query.TryParseDateOnly(nameof(SearchLeaveRequestsQueryDto.DateFrom)),
            DateTo: context.Request.Query.TryParseDateOnly(nameof(SearchLeaveRequestsQueryDto.DateTo))
         );
}
