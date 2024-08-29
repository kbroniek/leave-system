namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;

internal class CreateLeaveRequestValidator(BasicValidator basicValidator, ImpositionValidator impositionValidator, LimitValidator limitValidator)
{
    public virtual async Task<Result<Error>> Validate(LeaveRequestCreated @event, TimeSpan minDuration,
        TimeSpan maxDuration, bool? includeFreeDays)
    {
        var dataRangeResult = basicValidator.DataRangeValidate(@event);
        if (!dataRangeResult.IsSuccess)
        {
            return dataRangeResult;
        }
        var basicResult = basicValidator.Validate(@event, minDuration, maxDuration, includeFreeDays);
        if (!basicResult.IsSuccess)
        {
            return basicResult;
        }
        var limitResult = await limitValidator.Validate(@event);
        if (!limitResult.IsSuccess)
        {
            return limitResult;
        }
        var impositionResult = await impositionValidator.Validate(@event);
        if (!impositionResult.IsSuccess)
        {
            return impositionResult;
        }
        return Result.Default;
    }
}
