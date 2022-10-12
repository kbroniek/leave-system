using System.Net.Http.Json;

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
        var limits = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<UserLeaveLimitDto>>>($"odata/UserLeaveLimits?$select=Limit,OverdueLimit,LeaveTypeId&?$filter=AssignedToUserEmail eq '{userEmail}'");
        return limits?.Data ?? Enumerable.Empty<UserLeaveLimitDto>();
    }


    public record class UserLeaveLimitDto(TimeSpan Limit, TimeSpan OverdueLimit, Guid LeaveTypeId);
}

