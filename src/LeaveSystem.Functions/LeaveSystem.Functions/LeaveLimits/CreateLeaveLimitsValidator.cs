namespace LeaveSystem.Functions.LeaveLimits;

using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Functions.LeaveLimits.Repositories;
using LeaveSystem.Shared;

public class CreateLeaveLimitsValidator(ImpositionLimitRepository repository)
{
    internal async Task<Result<Error>> Validate(
        DateOnly? validSince, DateOnly? validUntil, string assignedToUserIds, Guid leaveTypeId, Guid? leaveLimitId, CancellationToken cancellationToken)
    {
        var result = await repository.GetLimits(validSince, validUntil, assignedToUserIds, leaveTypeId, leaveLimitId, 1, null, cancellationToken);
        if (result.Value.limits.Any())
        {
            return new Error("Limits already exists in that period", System.Net.HttpStatusCode.Conflict);
        }
        return Result.Default;
    }
}
