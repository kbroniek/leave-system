namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class CreateLeaveRequestValidator(BasicValidator basicValidator, ImpositionValidator impositionValidator, LimitValidator limitValidator)
{
    public virtual async Task<Result<Error>> Validate(
        DateOnly dateFrom,
        DateOnly dateTo,
        TimeSpan duration,
        Guid leaveTypeId,
        TimeSpan workingHours,
        string userId,
        CancellationToken cancellationToken)
    {
        var basicResult = await basicValidator.Validate(dateFrom, dateTo, duration, leaveTypeId, workingHours, cancellationToken);
        if (basicResult.IsFailure)
        {
            return basicResult;
        }
        var limitResult = await limitValidator.Validate(dateFrom, dateTo, duration, leaveTypeId, workingHours, userId, cancellationToken);
        if (limitResult.IsFailure)
        {
            return limitResult;
        }
        var impositionResult = await impositionValidator.Validate(dateFrom, dateTo, userId, cancellationToken);
        if (impositionResult.IsFailure)
        {
            return impositionResult;
        }
        return Result.Default;
    }
}
