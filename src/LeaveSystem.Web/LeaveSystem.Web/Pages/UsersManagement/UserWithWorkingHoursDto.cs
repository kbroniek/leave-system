using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Pages.UsersManagement;

public class UserWithWorkingHoursDto : UserDto
{
    public IEnumerable<WorkingHoursDto>? WorkingHours { get; set; }

    public UserWithWorkingHoursDto()
    {
        
    }

    public UserWithWorkingHoursDto(string? id, string? name, string? email, IEnumerable<string>? roles, IEnumerable<WorkingHoursDto> workingHours) : base(id, name, email, roles)
    {
        WorkingHours = workingHours;
    }

    public UserDto GetUser() => new(Id, Name, Email, Roles);
}