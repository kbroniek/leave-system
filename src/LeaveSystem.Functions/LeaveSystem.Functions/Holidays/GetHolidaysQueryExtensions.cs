namespace LeaveSystem.Functions.Holidays;

using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using Microsoft.AspNetCore.Http;

public static class GetHolidaysQueryExtensions
{
    public static Result<GetHolidaysQuery, Error> BindGetHolidaysQuery(this HttpContext context)
    {
        var dateFrom = context.Request.Query.ParseDateOnly(nameof(GetHolidaysQuery.DateFrom));
        var dateTo = context.Request.Query.ParseDateOnly(nameof(GetHolidaysQuery.DateTo));
        var results = new Result<object, Error>[] { dateFrom, dateTo };

        if (results.Any(x => x.IsFailure))
        {
            return CreateError(results);
        }

        return new GetHolidaysQuery(
            dateFrom, dateTo
        );
    }

    private static Error CreateError(Result<object, Error>[] results) =>
        new(
            string.Join(Environment.NewLine, results.Where(x => x.IsFailure).Select(x => x.Error.Message).Where(x => x is not null)),
            System.Net.HttpStatusCode.BadRequest);
}
