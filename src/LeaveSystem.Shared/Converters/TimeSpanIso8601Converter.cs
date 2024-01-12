using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace LeaveSystem.Shared.Converters;

public class TimeSpanIso8601Converter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value == null ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        var timeSpanValue = XmlConvert.ToString(value);
        writer.WriteStringValue(timeSpanValue);
    }
    
}