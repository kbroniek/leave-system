using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Api.Extensions;

public static class QueryCollectionExtensions
{
    public static string? TryParseString(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? (string?)paramValue : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to string type.", ex);
        }
    }
    public static string ParseString(this IQueryCollection query, string paramName)
    {
        var result = query.TryParseString(paramName);
        return result ?? throw new FormatException($"The {paramName} query parameter cannot be null.");
    }

    public static string[] TryParseStrings(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ?
                paramValue.Where(p => p != null).OfType<string>().ToArray() :
                Enumerable.Empty<string>().ToArray();
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to array of strings type.", ex);
        }
    }

    public static string[] ParseStrings(this IQueryCollection query, string paramName)
    {
        var result = query.TryParseStrings(paramName);
        return result.Length == 0 ? throw new FormatException($"The {paramName} query parameter cannot be null.") : result;
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

    public static WorkingHoursStatus[]? TryParseWorkingHoursStatuses(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? paramValue.Select(x => Enum.Parse<WorkingHoursStatus>(x!)).ToArray() : null;
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
    public static DateTimeOffset ParseDateTimeOffset(this IQueryCollection query, string paramName)
    {
        var result = query.TryParseDateTimeOffset(paramName);
        return result ?? throw new FormatException($"The {paramName} query parameter cannot be null.");
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
