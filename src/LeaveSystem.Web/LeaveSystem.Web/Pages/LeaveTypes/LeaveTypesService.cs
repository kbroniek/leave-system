using LeaveSystem.Shared;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeaveSystem.Web.Pages.LeaveTypes;

public class LeaveTypesService
{
    private readonly HttpClient httpClient;

    public LeaveTypesService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypes()
    {
        var leaveTypes = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<LeaveTypeDto>>>("odata/LeaveTypes?$select=Id,BaseLeaveTypeId,Name,Properties&$orderby=Order asc", new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new LeaveTypePropertiesConverter()
            }
        });
        return leaveTypes?.Data ?? Enumerable.Empty<LeaveTypeDto>();
    }

    public record class LeaveTypeDto(Guid Id, Guid? BaseLeaveTypeId, string Name, LeaveTypeProperties Properties);
    public record class LeaveTypeProperties(string? Color, LeaveTypeCatalog? Catalog, bool? IncludeFreeDays);

    private class LeaveTypePropertiesConverter : JsonConverter<LeaveTypeProperties>
    {
        public override LeaveTypeProperties Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var value = jsonDoc.RootElement.GetRawText();
                return value == null ?
                    new LeaveTypeProperties(null, null, null) :
                    JsonSerializer.Deserialize<LeaveTypeProperties>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)
                    {
                        Converters =
                        {
                            new LeaveTypeCatalogConverter()
                        }
                    }) ?? new LeaveTypeProperties(null, null, null);
            }
        }

        public override void Write(Utf8JsonWriter writer, LeaveTypeProperties value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(JsonSerializer.Serialize(value));
        }
    }
    private class LeaveTypeCatalogConverter : JsonConverter<LeaveTypeCatalog?>
    {
        public override LeaveTypeCatalog? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if(value == null)
            {
                return null;
            }
            return Enum.TryParse<LeaveTypeCatalog>(value, out LeaveTypeCatalog catalog) ? catalog : null;
        }

        public override void Write(Utf8JsonWriter writer, LeaveTypeCatalog? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(JsonSerializer.Serialize(value));
        }
    }
}

