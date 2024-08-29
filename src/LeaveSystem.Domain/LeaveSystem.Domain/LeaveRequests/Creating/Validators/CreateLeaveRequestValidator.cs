namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

using LeaveSystem.Domain.LeaveRequests.Creating;

internal class CreateLeaveRequestValidator(BasicValidator basicValidator, ImpositionValidator impositionValidator, LimitValidator limitValidator)
{
    public virtual async Task Validate(LeaveRequestCreated @event, TimeSpan minDuration,
        TimeSpan maxDuration, bool? includeFreeDays)
    {
        basicValidator.DataRangeValidate(@event);
        basicValidator.Validate(@event, minDuration, maxDuration, includeFreeDays);
        await impositionValidator.Validate(@event);
        await limitValidator.Validate(@event);
    }
}
