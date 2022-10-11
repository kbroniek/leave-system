using LeaveSystem.EventSourcing.LeaveRequests;

namespace LeaveSystem.Api.Endpoints;

public record class GetLeaveRequestsQuery(int? PageNumber, int? PageSize, DateTimeOffset? DateFrom, DateTimeOffset? DateTo,
    Guid[]? LeaveTypeIds, LeaveRequestStatus[]? Statuses, string[]? CreatedByEmails)
{
    public static ValueTask<GetLeaveRequestsQuery?> BindAsync(HttpContext context)
        => ValueTask.FromResult<GetLeaveRequestsQuery?>(new(
            PageNumber: TryParseInt(context.Request.Query, nameof(PageNumber)),
            PageSize: TryParseInt(context.Request.Query, nameof(PageSize)),
            DateFrom: TryParseDateTimeOffset(context.Request.Query, nameof(DateFrom)),
            DateTo: TryParseDateTimeOffset(context.Request.Query, nameof(DateTo)),
            LeaveTypeIds: TryParseGuids(context.Request.Query, nameof(LeaveTypeIds)),
            Statuses: TryParseLeaveRequestStatuses(context.Request.Query, nameof(Statuses)),
            CreatedByEmails: TryParseStrings(context.Request.Query, nameof(CreatedByEmails))
            ));

    private static string[]? TryParseStrings(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? paramValue.ToArray() : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to string type.", ex);
        }
    }

    private static LeaveRequestStatus[]? TryParseLeaveRequestStatuses(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? paramValue.Select(x => Enum.Parse<LeaveRequestStatus>(x)).ToArray() : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to LeaveRequestStatus type.", ex);
        }
    }

    private static Guid[]? TryParseGuids(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? paramValue.Select(x => Guid.Parse(x)).ToArray() : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to Guid type.", ex);
        }
    }

    private static DateTimeOffset? TryParseDateTimeOffset(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? DateTimeOffset.Parse(paramValue) : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to DateTimeOffset type.", ex);
        }
    }

    private static int? TryParseInt(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? int.Parse(paramValue) : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to int.", ex);
        }
    }
}
