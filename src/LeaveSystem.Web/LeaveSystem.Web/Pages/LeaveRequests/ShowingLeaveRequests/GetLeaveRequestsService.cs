using System.Net.Http.Json;
using System.Text.Json;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public class GetLeaveRequestsService
{
    private readonly HttpClient httpClient;

    public GetLeaveRequestsService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public virtual async Task<PagedListResponse<LeaveRequestShortInfo>?> GetLeaveRequests(GetLeaveRequestsQuery leaveRequestsQuery)
    {
        var uri = leaveRequestsQuery.CreateQueryString("api/leaveRequests");
        return await httpClient.GetFromJsonAsync<PagedListResponse<LeaveRequestShortInfo>>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}

