using System.Text.Json;
using System.Xml;

public class Iso8601DurationConverter : System.Text.Json.Serialization.JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) =>
        XmlConvert.ToTimeSpan(reader.GetString());

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
        writer.WriteStringValue(XmlConvert.ToString(value));
}
