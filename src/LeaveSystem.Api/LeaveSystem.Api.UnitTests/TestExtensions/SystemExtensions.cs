using System.Text.Json;

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
}