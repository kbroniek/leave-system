namespace LeaveSystem.Functions.LeaveLimits;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class CreateLeaveLimitsValidator
{
    internal async Task<Result<Error>> Validate(LeaveSystem.Shared.Dto.LeaveLimitDto leaveLimit, Guid? leaveLimitId = null)
    {
        return Result.Default;
    }
}
