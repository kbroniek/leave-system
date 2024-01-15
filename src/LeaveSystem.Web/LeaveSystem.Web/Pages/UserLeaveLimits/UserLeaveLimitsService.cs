namespace LeaveSystem.Web.Pages.UserLeaveLimits;

using System.Text.Json;
using System.Text.Json.Serialization;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.UserLeaveLimits;
using Shared;

public class UserLeaveLimitsService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TimeSpanIso8601Converter() }
    };

    private readonly UniversalHttpService universalHttpService;

    public UserLeaveLimitsService(UniversalHttpService universalHttpService) =>
        this.universalHttpService = universalHttpService;

    public virtual async Task<IEnumerable<UserLeaveLimitDto>> GetAsync(string userId, DateTimeOffset since,
        DateTimeOffset until)
    {
        var limits = await this.universalHttpService.GetAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>(
            $"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property&$filter=AssignedToUserId eq '{userId}' and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))"
            , "Can't get user leave limit", JsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<UserLeaveLimitDto>();
    }

    public virtual async Task<UserLeaveLimitDto?> GetAsync(Guid id)
    {
        var odataResponse = await this.universalHttpService.GetAsync<UserLeaveLimitDtoODataResponse>(
            $"odata/UserLeaveLimits({id})?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property",
            "Can't get user leave limit", JsonSerializerOptions);
        return odataResponse?.ToUserLeaveLimitDto();
    }

    public virtual async Task<IEnumerable<LeaveLimitDto>> GetAsync(DateTimeOffset since, DateTimeOffset until)
    {
        var limits = await this.universalHttpService.GetAsync<ODataResponse<IEnumerable<LeaveLimitDto>>>(
            $"odata/UserLeaveLimits?$select=Id,Limit,OverdueLimit,LeaveTypeId,ValidSince,ValidUntil,Property,AssignedToUserId&$filter=not(AssignedToUserId eq null) and ((ValidSince ge {since:s}Z or ValidSince eq null) and (ValidUntil le {until:s}.999Z or ValidUntil eq null))"
            , "Can't get user leave limit", JsonSerializerOptions);
        return limits?.Data ?? Enumerable.Empty<LeaveLimitDto>();
    }

    public async Task<UserLeaveLimitDto?> AddAsync(AddUserLeaveLimitDto entityToAdd)
    {
        var odataResponse =
            await this.universalHttpService.AddAsync<AddUserLeaveLimitDto, UserLeaveLimitDtoODataResponse>(
                "odata/UserLeaveLimits", entityToAdd, "User leave limit added successfully", JsonSerializerOptions);
        return odataResponse?.ToUserLeaveLimitDto();
    }

    public Task<bool> EditAsync(UserLeaveLimitDto entityToEdit) =>
        this.universalHttpService.EditAsync(
            $"odata/UserLeaveLimits({entityToEdit.Id})", entityToEdit, "User leave limit edited successfully",
            JsonSerializerOptions);

    public Task<bool> DeleteAsync(Guid id) =>
        this.universalHttpService.DeleteAsync($"odata/UserLeaveLimits({id})", "Successfully deleted leave limit",
            JsonSerializerOptions);

    public class UserLeaveLimitDtoODataResponse : UserLeaveLimitDto
    {
        [JsonPropertyName("@odata.context")] public string? ContextUrl { get; set; }

        public UserLeaveLimitDto ToUserLeaveLimitDto() =>
            new(this.Id, this.Limit, this.OverdueLimit, this.LeaveTypeId, this.ValidSince, this.ValidUntil,
                this.Property);
    }
}
