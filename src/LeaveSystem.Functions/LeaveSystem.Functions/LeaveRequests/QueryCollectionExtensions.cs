namespace LeaveSystem.Functions.LeaveRequests;
using System.Globalization;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using Microsoft.AspNetCore.Http;

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
            throw new BadHttpRequestException($"Cannot parse the {paramName} query parameter to string type.", StatusCodes.Status400BadRequest, ex);
        }
    }
    public static string ParseString(this IQueryCollection query, string paramName)
    {
        var result = query.TryParseString(paramName);
        return result ?? throw new BadHttpRequestException($"The {paramName} query parameter cannot be null.", StatusCodes.Status400BadRequest);
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
            throw new BadHttpRequestException($"Cannot parse the {paramName} query parameter to array of strings type.", StatusCodes.Status400BadRequest, ex);
        }
    }

    public static string[] ParseStrings(this IQueryCollection query, string paramName)
    {
        var result = query.TryParseStrings(paramName);
        return result.Length == 0 ? throw new BadHttpRequestException($"The {paramName} query parameter cannot be null.", StatusCodes.Status400BadRequest) : result;
    }

    public static LeaveRequestStatus[]? TryParseLeaveRequestStatuses(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ?
                paramValue.Where(x => x is not null).Select(x => Enum.Parse<LeaveRequestStatus>(x!)).ToArray() :
                null;
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException($"Cannot parse the {paramName} query parameter to LeaveRequestStatus type.", StatusCodes.Status400BadRequest, ex);
        }
    }

    public static WorkingHoursStatus[]? TryParseWorkingHoursStatuses(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ?
                paramValue.Where(x => x is not null).Select(x => Enum.Parse<WorkingHoursStatus>(x!)).ToArray() :
                null;
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException($"Cannot parse the {paramName} query parameter to LeaveRequestStatus type.", StatusCodes.Status400BadRequest, ex);
        }
    }
    public static Guid[]? TryParseGuids(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ?
                paramValue.Where(x => x is not null).Select(x => Guid.Parse(x!)).ToArray() :
                null;
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException($"Cannot parse the {paramName} query parameter to Guid type.", StatusCodes.Status400BadRequest, ex);
        }
    }

    public static DateOnly? TryParseDateOnly(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? DateOnly.Parse(paramValue!, CultureInfo.CurrentCulture) : null;
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException($"Cannot parse the {paramName} query parameter to DateTimeOffset type.", StatusCodes.Status400BadRequest, ex);
        }
    }
    public static DateOnly ParseDateTimeOffset(this IQueryCollection query, string paramName)
    {
        var result = query.TryParseDateOnly(paramName);
        return result ?? throw new BadHttpRequestException($"The {paramName} query parameter cannot be null.", StatusCodes.Status400BadRequest);
    }

    public static int? TryParseInt(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? int.Parse(paramValue!, CultureInfo.CurrentCulture) : null;
        }
        catch (Exception ex)
        {
            throw new BadHttpRequestException($"Cannot parse the {paramName} query parameter to int.", StatusCodes.Status400BadRequest, ex);
        }
    }
}
