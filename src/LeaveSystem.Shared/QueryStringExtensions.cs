using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Globalization;

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
        if (value == null)
        {
            return "";
        }
        if (value is DateTimeOffset date)
        {
            return date.ToString("o", CultureInfo.InvariantCulture);
        }
        if (value is IEnumerable collection)
        {
            return new StringValues(collection.Cast<object>().Where(x => x != null).Select(x => x.ToString()).ToArray());
        }
        return value.ToString();
    }
}

