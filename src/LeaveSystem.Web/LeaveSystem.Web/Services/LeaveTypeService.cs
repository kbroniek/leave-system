using System.Net.Http.Json;

namespace LeaveSystem.Web.Services;

public class LeaveTypeService
{
    private readonly HttpClient httpClient;

    public LeaveTypeService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypes()
    {
        var leaveTypes = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<LeaveTypeDto>>>("odata/LeaveTypes?$select=Id,Name&$orderby=Order asc");
        return leaveTypes?.Data ?? Enumerable.Empty<LeaveTypeDto>();
    }


    public class LeaveTypeDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}

