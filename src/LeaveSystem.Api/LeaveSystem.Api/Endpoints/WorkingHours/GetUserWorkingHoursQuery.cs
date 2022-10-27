using LeaveSystem.Api.Extensions;

namespace LeaveSystem.Api.Endpoints.WorkingHours;

public record class GetUserWorkingHoursQuery(DateTimeOffset DateFrom, DateTimeOffset DateTo)
{
    public static ValueTask<GetUserWorkingHoursQuery?> BindAsync(HttpContext context)
        => ValueTask.FromResult<GetUserWorkingHoursQuery?>(new(
            DateFrom: context.Request.Query.ParseDateTimeOffset(nameof(DateFrom)),
            DateTo: context.Request.Query.ParseDateTimeOffset(nameof(DateTo))
       ));
}
