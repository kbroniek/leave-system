using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Pages.UsersManagement;

public class UserWithWorkingHoursDto : UserDto
{
    public List<WorkingHoursDto>? WorkingHours { get; set; }

    public UserWithWorkingHoursDto()
    {
        WorkingHours = new List<WorkingHoursDto>();
    }

    public UserWithWorkingHoursDto(string? id, string? name, string? email, IEnumerable<string>? roles, IEnumerable<WorkingHoursDto> workingHours) : base(id, name, email, roles)
    {
        WorkingHours = workingHours.ToList();
    }

    public UserDto GetUser() => new(Id, Name, Email, Roles);


    public string TimeProxy
    {
        get => CurrentWorkingHours?.ToString() ?? "";
        set
        {
            TimeSpan.TryParse(value, out var parsedValue);
        }
    }


    public TimeSpan? CurrentWorkingHours
    {
        get
        {
            DateTimeOffset now = DateTimeOffset.Now.GetDayWithoutTime();
            return WorkingHours?.FirstOrDefault(x => x.DateFrom <= now && (x.DateTo >= now || x.DateTo is null))?.Duration;
        }
    }
}