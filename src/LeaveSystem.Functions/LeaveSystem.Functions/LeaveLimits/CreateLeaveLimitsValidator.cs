namespace LeaveSystem.Functions.LeaveLimits;

using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Functions.LeaveLimits.Repositories;
using LeaveSystem.Shared;

public class CreateLeaveLimitsValidator(ImpositionLimitRepository repository)
{
    internal async Task<Result<Error>> Validate(
        DateOnly? validSince, DateOnly? validUntil, string? assignedToUserId, Guid leaveTypeId, Guid? leaveLimitId, CancellationToken cancellationToken)
    {
        if (validSince is not null && validUntil is not null)
        {
            if (validSince > validUntil)
            {
                return Error.BadRequest("validSince > validUntil");
            }
            if (validSince.Value.Year != validUntil.Value.Year)
            {
                return Error.BadRequest("validSince.Value.Year != validUntil.Value.Year");
            }
        }
        //TODO: Check if leave type exists and if user exists.
        var result = await repository.GetLimits(validSince, validUntil, assignedToUserId, leaveTypeId, leaveLimitId, 1, null, cancellationToken);
        if (result.Value.limits.Any())
        {
            return Error.Conflict("Limits already exists in that period");
        }
        return Result.Default;
    }
}
