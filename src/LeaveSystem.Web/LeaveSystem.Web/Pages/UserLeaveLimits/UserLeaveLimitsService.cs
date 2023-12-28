using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Blazored.Toast.Services;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.UserLeaveLimits;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Shared;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class UserLeaveLimitsService
{
    private readonly UniversalHttpService universalHttpService;
    
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
            {
                new TimeSpanIso8601Converter()
            },
    };

    public UserLeaveLimitsService(UniversalHttpService universalHttpService)
    {
        this.universalHttpService = universalHttpService;
    }

    public virtual async Task<IEnumerable<UserLeaveLimitDto>> GetLimits(string userId, DateTimeOffset since, DateTimeOffset until)
    {
        var limits = await universalHttpService.GetAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>(
            $"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))"
            , "Can't get user leave limit", jsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<UserLeaveLimitDto>();
    }

    public virtual async Task<UserLeaveLimitDto?> GetSingleAsync(Guid id)
    {
        var odataResponse = await universalHttpService.GetAsync<UserLeaveLimitDtoODataResponse>(
            $"odata/UserLeaveLimits({id})?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property",
            "Can't get user leave limit", jsonSerializerOptions);
        return odataResponse?.ToUserLeaveLimitDto();
    }

    public virtual async Task<IEnumerable<LeaveLimitDto>> GetLimits(DateTimeOffset since, DateTimeOffset until)
    {
        var limits = await universalHttpService.GetAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>(
            $"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property,AssignedToUserId&$filter=not(AssignedToUserId eq null) and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))"
            , "Can't get user leave limit", jsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<LeaveLimitDto>();
    }

    public async Task<UserLeaveLimitDto?> AddAsync(AddUserLeaveLimitDto entityToAdd)
    {
        var odataResponse = await universalHttpService.AddAsync<AddUserLeaveLimitDto, UserLeaveLimitDtoODataResponse>("odata/UserLeaveLimits", entityToAdd, "User leave limit added successfully", jsonSerializerOptions);
        return odataResponse?.ToUserLeaveLimitDto();
    } 
    public Task<bool> EditAsync(UserLeaveLimitDto entityToEdit) => universalHttpService.EditAsync($"odata/UserLeaveLimits({entityToEdit.Id})", entityToEdit, "User leave limit edited successfully", jsonSerializerOptions);

    public Task<bool> DeleteAsync(Guid id) =>
        universalHttpService.DeleteAsync($"odata/UserLeaveLimits({id})", "Successfully deleted leave limit", jsonSerializerOptions);
    public class UserLeaveLimitDtoODataResponse : UserLeaveLimitDto
    {
        [JsonPropertyName(name: "@odata.context")]
        public string? ContextUrl { get; set; }
        public UserLeaveLimitDto ToUserLeaveLimitDto() =>
            new(Id, Limit, OverdueLimit, LeaveTypeId, ValidSince, ValidUntil, Property);
    }
}

