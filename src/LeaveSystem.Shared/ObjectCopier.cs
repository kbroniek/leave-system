using System.Text.Json;

namespace LeaveSystem.Shared;

public static class ObjectCopier
{
    public static T Copy<T>(T source)
    {
        var serializedSource = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<T>(serializedSource)!;
    }
}