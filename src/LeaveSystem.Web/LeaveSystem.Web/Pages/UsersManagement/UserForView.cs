using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Pages.UsersManagement;

public class UserForView
{
    public static UserForView Create() => new UserForView(UserDto.Create(), new WorkingHoursDto());

    public UserForView(UserDto user, WorkingHoursDto? workingHours)
    {
        this.User = user;
        this.WorkingHours = workingHours;
    }

    public bool CreateWorkingHoursViewVisible { get; set; }

    public UserDto User { get; set; }
    public WorkingHoursDto? WorkingHours { get; set; }
}
