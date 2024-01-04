using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveSystem.Converters;

internal class TypeToJsonConverter<T> : ValueConverter<T, string>
{
    public TypeToJsonConverter() : base(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
            v => JsonSerializer.Deserialize<T>(v, new JsonSerializerOptions())!
        )
    {
    }
}
