using LeaveSystem.Shared;
using System.Globalization;

namespace LeaveSystem.Services;

public class WorkingHoursService
{
    public virtual ValueTask<TimeSpan> GetUsersWorkingHours(FederatedUser user)
    {
        return ValueTask.FromResult(TimeSpan.FromHours(8));
    }
}

