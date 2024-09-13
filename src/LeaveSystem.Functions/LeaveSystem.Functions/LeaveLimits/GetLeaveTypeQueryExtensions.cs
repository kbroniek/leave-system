namespace LeaveSystem.Functions.LeaveLimits;

using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Http;

public static class GetLeaveTypeQueryExtensions
{
    public static Result<SearchLeaveLimitQuery, Error> BindSearchLeaveLimitQuery(this HttpContext context)
    {
        var year = context.Request.Query.Int(nameof(SearchLeaveLimitQuery.Year));
        var userIds = context.Request.Query.TryStrings(nameof(SearchLeaveLimitQuery.UserIds));
        var leaveTypeIds = context.Request.Query.TryGuids(nameof(SearchLeaveLimitQuery.LeaveTypeIds));
        var pageSize = context.Request.Query.TryInt(nameof(SearchLeaveLimitQuery.PageSize));
        var continuationToken = context.Request.Query.TryString(nameof(SearchLeaveLimitQuery.ContinuationToken));
        var results = new Result<object, Error>[] { year, userIds, leaveTypeIds, pageSize, continuationToken };

        if (results.Any(x => x.IsFailure))
        {
            return CreateError(results);
        }

        return new SearchLeaveLimitQuery(
            Year: year.Value,
            UserIds: userIds.Value,
            LeaveTypeIds: leaveTypeIds.Value,
            PageSize: pageSize.Value,
            ContinuationToken: continuationToken.Value
        );
    }

    public static Result<SearchUserLeaveLimitQuery, Error> BindSearchUserLeaveLimitQuery(this HttpContext context)
    {
        var year = context.Request.Query.Int(nameof(SearchLeaveLimitQuery.Year));
        var leaveTypeIds = context.Request.Query.TryGuids(nameof(SearchLeaveLimitQuery.LeaveTypeIds));
        var pageSize = context.Request.Query.TryInt(nameof(SearchLeaveLimitQuery.PageSize));
        var continuationToken = context.Request.Query.TryString(nameof(SearchLeaveLimitQuery.ContinuationToken));
        var results = new Result<object, Error>[] { year, leaveTypeIds, pageSize, continuationToken };

        if (results.Any(x => x.IsFailure))
        {
            return CreateError(results);
        }

        return new SearchUserLeaveLimitQuery(
            Year: year.Value,
            LeaveTypeIds: leaveTypeIds.Value,
            PageSize: pageSize.Value,
            ContinuationToken: continuationToken.Value
        );
    }

    private static Error CreateError(Result<object, Error>[] results) =>
        new(
            string.Join(Environment.NewLine, results.Select(x => x.Error.Message).Where(x => x is not null)),
            System.Net.HttpStatusCode.BadRequest);
}
