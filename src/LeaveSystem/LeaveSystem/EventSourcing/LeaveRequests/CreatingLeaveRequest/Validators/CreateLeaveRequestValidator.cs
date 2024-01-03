namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class CreateLeaveRequestValidator
{
    private readonly BasicValidator basicValidator;
    private readonly ImpositionValidator impositionValidator;
    private readonly LimitValidator limitValidator;

    public CreateLeaveRequestValidator(BasicValidator basicValidator, ImpositionValidator impositionValidator, LimitValidator limitValidator)
    {
        this.basicValidator = basicValidator;
        this.impositionValidator = impositionValidator;
        this.limitValidator = limitValidator;
    }

    public async virtual Task Validate(LeaveRequestCreated @event, TimeSpan minDuration,
        TimeSpan maxDuration, bool? includeFreeDays)
    {
        basicValidator.DataRangeValidate(@event);
        basicValidator.Validate(@event, minDuration, maxDuration, includeFreeDays);
        await impositionValidator.Validate(@event);
        await limitValidator.Validate(@event);
    }
}