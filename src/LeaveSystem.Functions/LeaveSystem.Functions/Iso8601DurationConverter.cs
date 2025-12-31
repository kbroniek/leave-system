using System.Text.Json;
using System.Xml;

namespace LeaveSystem.Functions;

public class Iso8601DurationConverter : System.Text.Json.Serialization.JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value is null ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value != TimeSpan.Zero ? XmlConvert.ToString(value) : null);
}
