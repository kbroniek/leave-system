namespace LeaveSystem.Functions.Extensions;
using LeaveSystem.Domain;
using LeaveSystem.Shared;
using Microsoft.AspNetCore.Http;

public static class HttpQueryExtensions
{
    public static Result<string?, Error> TryString(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        if (string.IsNullOrEmpty(paramValue))
        {
            return null;
        }
        return paramValue.ToString();
    }
    public static Result<string[], Error> TryStrings(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        return !string.IsNullOrEmpty(paramValue) ?
            paramValue.Where(p => p != null).Select(p => p.ToString()).ToArray() :
            [];
    }

    public static Result<Guid[], Error> TryGuids(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        var results = !string.IsNullOrEmpty(paramValue) ?
            paramValue.Where(x => x is not null).Select(ParseGuid).ToArray() :
            [];
        var failureResults = results.Where(x => x.IsFailure).ToArray();
        if (failureResults.Length > 0)
        {
            var serializedErrors = string.Join(Environment.NewLine, failureResults.Select(x => x.Error.Message));
            return new Error($"Cannot parse the {paramName} query parameter to Guid type.{Environment.NewLine}{serializedErrors}", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT);
        }
        return results.Select(x => x.Value).ToArray();
    }

    private static Result<Guid, Error> ParseGuid(string? input) =>
        Guid.TryParse(input, out var result) ?
        result :
        new Error($"Cannot parse the {input}.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT);

    public static Result<int?, Error> TryInt(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        if (string.IsNullOrEmpty(paramValue))
        {
            return null;
        }
        return int.TryParse(paramValue, out var result) ?
            result :
            new Error($"Cannot parse the {paramName} query parameter to int.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT);
    }

    public static Result<int, Error> Int(this IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        return int.TryParse(paramValue, out var result) ?
            result :
            new Error($"Cannot parse the {paramName} query parameter to int.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT);
    }
}
