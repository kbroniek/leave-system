using LeaveSystem.EventSourcing.LeaveRequests;

namespace LeaveSystem.Api.Extensions;

public static class QueryCollectionExtensions
{
    public static string[]? TryParseStrings(this IQueryCollection query, string paramName)
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

    public static LeaveRequestStatus[]? TryParseLeaveRequestStatuses(this IQueryCollection query, string paramName)
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

    public static Guid[]? TryParseGuids(this IQueryCollection query, string paramName)
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

    public static DateTimeOffset? TryParseDateTimeOffset(this IQueryCollection query, string paramName)
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

    public static int? TryParseInt(this IQueryCollection query, string paramName)
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
