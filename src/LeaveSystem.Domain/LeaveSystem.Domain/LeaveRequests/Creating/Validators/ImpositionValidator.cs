namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class ImpositionValidator(IImpositionValidatorRepository impositionValidatorRepository)
{
    public virtual async Task<Result<Error>> Validate(
        DateOnly dateFrom,
        DateOnly dateTo,
        string userId,
        CancellationToken cancellationToken)
    {
        if (await impositionValidatorRepository.IsExistValid(userId, dateFrom, dateTo, cancellationToken))
        {
            return new Error("Cannot create a new leave request in this time. The other leave is overlapping with this date", System.Net.HttpStatusCode.Conflict);
        }
        return Result.Default;
    }
}
public interface IImpositionValidatorRepository
{
    ValueTask<bool> IsExistValid(string createdById, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken);
}
