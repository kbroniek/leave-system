using System.Text.Json;
using Microsoft.AspNetCore.OData.Deltas;

namespace LeaveSystem.Api.UnitTests.TestExtensions;

public static class SystemExtensions
{
    public static T? Clone<T>(this T source)
    {
        if (source is null)
        {
            return default;
        }
        var serializedCopy = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<T>(serializedCopy);
    }

    public static Delta<T> ToDelta<T>(this T source) where T : class
    {
        var delta = new Delta<T>();
        var properties = source.GetType().GetProperties();
        foreach (var property in properties)
        {
            delta.TrySetPropertyValue(property.Name, property.GetValue(source, null));
        }
        return delta;
    }

    public static Delta<T> ToDelta<T>(this object source) where T : class
    {
        var delta = new Delta<T>();
        var properties = source.GetType().GetProperties();
        foreach (var property in properties)
        {
            delta.TrySetPropertyValue(property.Name, property.GetValue(source, null));
        }
        return delta;
    }
}