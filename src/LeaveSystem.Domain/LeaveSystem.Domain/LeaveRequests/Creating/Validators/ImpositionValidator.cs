namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class ImpositionValidator(IImpositionValidatorRepository impositionValidatorRepository)
{
    public virtual async Task<Result<Error>> Validate(
        Guid leaveRequestId, DateOnly dateFrom, DateOnly dateTo,
        string userId, CancellationToken cancellationToken)
    {
        if (await impositionValidatorRepository.IsExistValid(leaveRequestId, userId, dateFrom, dateTo, cancellationToken))
        {
            return new Error("The other leave is overlapping with this date.", System.Net.HttpStatusCode.Conflict);
        }
        return Result.Default;
    }
}
public interface IImpositionValidatorRepository
{
    ValueTask<bool> IsExistValid(Guid leaveRequestId, string createdById, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken);
}
