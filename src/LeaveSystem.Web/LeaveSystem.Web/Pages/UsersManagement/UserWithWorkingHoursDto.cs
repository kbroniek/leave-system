using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Pages.UsersManagement;

public class UserWithWorkingHoursDto : UserDto
{
    public List<WorkingHoursDto>? WorkingHours { get; set; }

    public UserWithWorkingHoursDto()
    {
        
    }

    public UserWithWorkingHoursDto(string? id, string? name, string? email, IEnumerable<string>? roles, IEnumerable<WorkingHoursDto> workingHours) : base(id, name, email, roles)
    {
        WorkingHours = workingHours.ToList();
    }

    public UserDto GetUser() => new(Id, Name, Email, Roles);
}