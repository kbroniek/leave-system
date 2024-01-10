using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;

namespace LeaveSystem.Web.Pages.WorkingHours;

using LeaveSystem.Shared.Converters;
using Shared;

public class WorkingHoursService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly UniversalHttpService universalHttpService;

    public WorkingHoursService(UniversalHttpService universalHttpService) => this.universalHttpService = universalHttpService;

    public virtual Task<PagedListResponse<WorkingHoursDto>?> GetWorkingHours(GetWorkingHoursQuery query) =>
        this.universalHttpService.GetAsync<PagedListResponse<WorkingHoursDto>>(
            query.CreateQueryString("api/workingHours"),
            "Error occured during getting working hours",
            JsonSerializerOptions);

    public virtual Task<WorkingHoursDto?> GetUserWorkingHours(string userId) =>
        this.universalHttpService.GetAsync<WorkingHoursDto>(
            $"api/workingHours/{userId}",
            "Error occured during getting working hours",
            JsonSerializerOptions);

    public virtual Task<bool> EditAsync(WorkingHoursDto workingHoursDto) =>
        this.universalHttpService.PutAsync(
            $"api/workingHours/{workingHoursDto.Id}/modify",
            workingHoursDto,
            "Edited working hours successfully",
            JsonSerializerOptions);

    public virtual Task<WorkingHoursDto?> AddAsync(AddWorkingHoursDto addWorkingHoursDto) =>
        this.universalHttpService.PostAsync<AddWorkingHoursDto, WorkingHoursDto>(
            "api/workingHours",
            addWorkingHoursDto,
            "Added working hours successfully",
            JsonSerializerOptions);
}
