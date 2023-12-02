using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Blazored.Toast.Services;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Shared;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class UserLeaveLimitsService : UniversalHttpService<UserLeaveLimitDto>
{
    private readonly HttpClient httpClient;
    private readonly IToastService toastService;
    private readonly ILogger<UserLeaveLimitsService> logger;
    
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
            {
                new TimeSpanToStringConverter()
            }
    };

    public UserLeaveLimitsService(HttpClient httpClient, IToastService toastService, ILogger<UserLeaveLimitsService> logger) 
        : base(httpClient, toastService, logger, "User leave limit")
    {
        this.httpClient = httpClient;
        this.toastService = toastService;
        this.logger = logger;
    }

    public virtual async Task<IEnumerable<UserLeaveLimitDto>> GetLimits(string userId, DateTimeOffset since, DateTimeOffset until)
    {
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>($"odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))", jsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<UserLeaveLimitDto>();
    }

    public virtual async Task<IEnumerable<LeaveLimitDto>> GetLimits(DateTimeOffset since, DateTimeOffset until)
    {
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>($"odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property,AssignedToUserId&$filter=not(AssignedToUserId eq null) and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))", jsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<LeaveLimitDto>();
    }

    public Task<UserLeaveLimitDto?> AddAsync(UserLeaveLimitDto entityToAdd) => AddAsync("odata/UserLeaveLimits", entityToAdd, "User leave limit added successfully");
    public Task<bool> EditAsync(UserLeaveLimitDto entityToEdit) => EditAsync("odata/UserLeaveLimits", entityToEdit, "User leave limit edited successfully");

    private sealed class TimeSpanToStringConverter : JsonConverter<TimeSpan>
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

