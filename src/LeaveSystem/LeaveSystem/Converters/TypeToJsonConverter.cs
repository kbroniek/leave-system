using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveSystem.Converters;

internal class TypeToJsonConverter<T> : ValueConverter<T, string>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new();
    public TypeToJsonConverter() : base(
            v => JsonSerializer.Serialize(v, JsonSerializerOptions),
            v => JsonSerializer.Deserialize<T>(v, JsonSerializerOptions)!
        )
    {
    }
}
