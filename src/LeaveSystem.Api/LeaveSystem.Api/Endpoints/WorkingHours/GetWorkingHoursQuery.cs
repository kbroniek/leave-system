using LeaveSystem.Api.Extensions;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Api.Endpoints.WorkingHours;

public record GetWorkingHoursQuery(int? PageNumber, int? PageSize, WorkingHoursStatus[]? Statuses, string[]? UserIds, DateTimeOffset DateFrom, DateTimeOffset DateTo)
{
    public static ValueTask<GetWorkingHoursQuery?> BindAsync(HttpContext context)
        => ValueTask.FromResult<GetWorkingHoursQuery?>(new(
            PageNumber: context.Request.Query.TryParseInt(nameof(PageNumber)),
            PageSize: context.Request.Query.TryParseInt(nameof(PageSize)),
            UserIds: context.Request.Query.ParseStrings(nameof(UserIds)),
            DateFrom: context.Request.Query.ParseDateTimeOffset(nameof(DateFrom)),
            DateTo: context.Request.Query.ParseDateTimeOffset(nameof(DateTo)),
            Statuses: context.Request.Query.TryParseWorkingHoursStatuses(nameof(Statuses))
       ));
}
