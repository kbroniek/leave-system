using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class UserLeaveLimitsService
{
    private readonly HttpClient httpClient;

    public UserLeaveLimitsService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<IEnumerable<UserLeaveLimitDto>> GetLimits(string userEmail)
    {
        // TODO: FIX. Returns only data for one user.
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>($"odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId&?$filter=AssignedToUserEmail eq '{userEmail}'", new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new TimeSpanToStringConverter()
            }
        });
        return limits?.Data ?? Enumerable.Empty<UserLeaveLimitDto>();
    }


    public record class UserLeaveLimitDto(TimeSpan Limit, TimeSpan OverdueLimit, Guid LeaveTypeId);

    private class TimeSpanToStringConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value == null ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

