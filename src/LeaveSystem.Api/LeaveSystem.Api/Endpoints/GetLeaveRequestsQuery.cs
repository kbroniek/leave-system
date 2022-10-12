﻿using LeaveSystem.Api.Extensions;
using LeaveSystem.EventSourcing.LeaveRequests;

namespace LeaveSystem.Api.Endpoints;

public record class GetLeaveRequestsQuery(int? PageNumber, int? PageSize, DateTimeOffset? DateFrom, DateTimeOffset? DateTo,
    Guid[]? LeaveTypeIds, LeaveRequestStatus[]? Statuses, string[]? CreatedByEmails)
{
    public static ValueTask<GetLeaveRequestsQuery?> BindAsync(HttpContext context)
        => ValueTask.FromResult<GetLeaveRequestsQuery?>(new(
            PageNumber: context.Request.Query.TryParseInt(nameof(PageNumber)),
            PageSize: context.Request.Query.TryParseInt(nameof(PageSize)),
            DateFrom: context.Request.Query.TryParseDateTimeOffset(nameof(DateFrom)),
            DateTo: context.Request.Query.TryParseDateTimeOffset(nameof(DateTo)),
            LeaveTypeIds: context.Request.Query.TryParseGuids(nameof(LeaveTypeIds)),
            Statuses: context.Request.Query.TryParseLeaveRequestStatuses(nameof(Statuses)),
            CreatedByEmails: context.Request.Query.TryParseStrings(nameof(CreatedByEmails))
            ));
}
