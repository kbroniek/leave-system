using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Blazored.Toast.Services;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Shared;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class UserLeaveLimitsService : UniversalHttpService
{
    private readonly HttpClient httpClient;
    
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
            {
                new TimeSpanToStringConverter()
            },
    };

    public UserLeaveLimitsService(HttpClient httpClient, IToastService toastService, ILogger<UserLeaveLimitsService> logger) 
        : base(httpClient, toastService, logger)
    {
        this.httpClient = httpClient;
    }

    public virtual async Task<IEnumerable<UserLeaveLimitDto>> GetLimits(string userId, DateTimeOffset since, DateTimeOffset until)
    {
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>($"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))", jsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<UserLeaveLimitDto>();
    }

    public virtual async Task<UserLeaveLimitDto?> GetSingleAsync(Guid id)
    {
        var odataResponse = await GetAsync<UserLeaveLimitDtoODataResponse>(
            $"odata/UserLeaveLimits({id})?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property",
            "Can't get user leave limit", jsonSerializerOptions);
        return odataResponse?.ToUserLeaveLimitDto();
    }

    public virtual async Task<IEnumerable<LeaveLimitDto>> GetLimits(DateTimeOffset since, DateTimeOffset until)
    {
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>($"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property,AssignedToUserId&$filter=not(AssignedToUserId eq null) and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))", jsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<LeaveLimitDto>();
    }

    public async Task<UserLeaveLimitDto?> AddAsync(UserLeaveLimitDto entityToAdd)
    {
        var odataResponse = await AddAsync<UserLeaveLimitDto, UserLeaveLimitDtoODataResponse>("odata/UserLeaveLimits", entityToAdd, "User leave limit added successfully", jsonSerializerOptions);
        return odataResponse?.ToUserLeaveLimitDto();
    } 
    public Task<bool> EditAsync(UserLeaveLimitDto entityToEdit) => EditAsync($"odata/UserLeaveLimits({entityToEdit.Id})", entityToEdit, "User leave limit edited successfully", jsonSerializerOptions);
    
    private class UserLeaveLimitDtoODataResponse : UserLeaveLimitDto
    {
        [JsonPropertyName(name: "@odata.context")]
        public string? ContextUrl { get; set; }
        public UserLeaveLimitDto ToUserLeaveLimitDto() =>
            new(Id, Limit, OverdueLimit, LeaveTypeId, ValidSince, ValidUntil, Property);
    }

    private sealed class TimeSpanToStringConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value == null ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(XmlConvert.ToString(value));
        }
    }
}

