using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace LeaveSystem.Shared;

public static class QueryStringExtensions
{
    public static string CreateQueryString(this object baseObject, string baseUrl)
    {
        var keyValuePairs = baseObject.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => ToString(p.GetValue(baseObject)));
        return baseUrl + QueryString.Create(keyValuePairs).Value;
    }

    private static StringValues ToString(object? value)
    {
        return value switch
        {
            null => "",
            DateTimeOffset date => date.ToString("o", CultureInfo.InvariantCulture),
            IEnumerable collection => new StringValues(collection.Cast<object>()
                .Where(x => x != null)
                .Select(x => x.ToString())
                .ToArray()),
            _ => value.ToString()
        };
    }
}