using Microsoft.AspNetCore.Http;

public static class QueryStringExtensions
{
    public static QueryString CreateQueryString(this object value)
    {
        var keyValuePairs = value.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(value)?.ToString());
        return QueryString.Create(keyValuePairs);
    }
}

