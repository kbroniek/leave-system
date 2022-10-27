using LeaveSystem.Api.Extensions;

namespace LeaveSystem.Api.Endpoints.WorkingHours;

public record class GetWorkingHoursQuery(string[] UserEmails, DateTimeOffset DateFrom, DateTimeOffset DateTo)
{
    public static ValueTask<GetWorkingHoursQuery?> BindAsync(HttpContext context)
        => ValueTask.FromResult<GetWorkingHoursQuery?>(new(
            UserEmails: context.Request.Query.ParseStrings(nameof(UserEmails)),
            DateFrom: context.Request.Query.ParseDateTimeOffset(nameof(DateFrom)),
            DateTo: context.Request.Query.ParseDateTimeOffset(nameof(DateTo))
       ));
}
