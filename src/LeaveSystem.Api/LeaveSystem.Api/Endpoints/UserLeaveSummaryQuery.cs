using LeaveSystem.Api.Extensions;

namespace LeaveSystem.Api.Endpoints;
public record class UserLeaveSummaryQuery(DateTimeOffset? DateFrom, DateTimeOffset? DateTo)
{
    public static ValueTask<UserLeaveSummaryQuery?> BindAsync(HttpContext context)
       => ValueTask.FromResult<UserLeaveSummaryQuery?>(new(
           DateFrom: context.Request.Query.TryParseDateTimeOffset(nameof(DateFrom)),
           DateTo: context.Request.Query.TryParseDateTimeOffset(nameof(DateTo))
      ));
}
