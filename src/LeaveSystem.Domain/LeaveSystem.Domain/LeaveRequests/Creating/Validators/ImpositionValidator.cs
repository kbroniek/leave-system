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
        if (await impositionValidatorRepository.ExistValid(userId, dateFrom, dateTo))
        {
            return new Error("Cannot create a new leave request in this time. The other leave is overlapping with this date", System.Net.HttpStatusCode.BadRequest);
        }
        return Result.Ok<Error>();
    }
}
public interface IImpositionValidatorRepository
{
    ValueTask<bool> ExistValid(string createdById, DateOnly dateFrom, DateOnly dateTo);
}
