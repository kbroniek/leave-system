using LeaveSystem.Api.Extensions;

namespace LeaveSystem.Api.Endpoints.WorkingHours;

public record class GetWorkingHoursQuery(string[] UserIds, DateTimeOffset DateFrom, DateTimeOffset DateTo)
{
    public static ValueTask<GetWorkingHoursQuery?> BindAsync(HttpContext context)
        => ValueTask.FromResult<GetWorkingHoursQuery?>(new(
            UserIds: context.Request.Query.ParseStrings(nameof(UserIds)),
            DateFrom: context.Request.Query.ParseDateTimeOffset(nameof(DateFrom)),
            DateTo: context.Request.Query.ParseDateTimeOffset(nameof(DateTo))
       ));
}
