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
    //TODO: add time range
    public async Task<IEnumerable<UserLeaveLimitDto>> GetLimits(string userEmail)
    {
        // TODO: FIX. Returns only data for one user.
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>($"odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserEmail eq '{userEmail}'", new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new TimeSpanToStringConverter()
            }
        });
        return limits?.Data ?? Enumerable.Empty<UserLeaveLimitDto>();
    }
    //TODO: add time range
    public async Task<IEnumerable<LeaveLimitDto>> GetLimits()
    {
        // TODO: FIX. Returns only data for one user.
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>("odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property,AssignedToUserEmail&$filter=not(AssignedToUserEmail eq null)", new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new TimeSpanToStringConverter()
            }
        });
        return limits?.Data ?? Enumerable.Empty<LeaveLimitDto>();
    }

    public record class LeaveLimitDto(TimeSpan Limit, TimeSpan OverdueLimit, Guid LeaveTypeId, DateTimeOffset? ValidSince, DateTimeOffset? ValidUntil, string AssignedToUserEmail)
    {
        public TimeSpan TotalLimit { get => Limit + OverdueLimit; }
    }
    public record class UserLeaveLimitDto(TimeSpan Limit, TimeSpan OverdueLimit, Guid LeaveTypeId, DateTimeOffset? ValidSince, DateTimeOffset? ValidUntil, UserLeaveLimitPropertyDto? Property);

    public record class UserLeaveLimitPropertyDto(string? Description);

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

