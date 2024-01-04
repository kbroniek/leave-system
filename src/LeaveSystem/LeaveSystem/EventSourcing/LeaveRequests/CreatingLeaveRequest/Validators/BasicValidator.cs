using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Date;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class BasicValidator
{
    private readonly CurrentDateService currentDateService;

    public BasicValidator(CurrentDateService currentDateService)
    {
        this.currentDateService = currentDateService;
    }
    public virtual void DataRangeValidate(LeaveRequestCreated @event)
    {
        var dateFromWithoutTime = @event.DateFrom.GetDayWithoutTime();
        var dateToWithoutTime = @event.DateTo.GetDayWithoutTime();
        var now = currentDateService.UtcNowWithoutTime();
        var firstDay = now.GetFirstDayOfYear();
        var lastDay = now.GetLastDayOfYear();
        Guard.Against.OutOfRange(dateFromWithoutTime, nameof(@event.DateFrom), firstDay, lastDay);
        Guard.Against.OutOfRange(dateToWithoutTime, nameof(@event.DateTo), firstDay, lastDay);
        if (dateFromWithoutTime > dateToWithoutTime)
        {
            throw new ArgumentOutOfRangeException(nameof(@event.DateFrom),
                "Date from has to be less than date to.");
        }
    }
    public virtual void Validate(LeaveRequestCreated @event, TimeSpan minDuration,
        TimeSpan maxDuration, bool? includeFreeDays)
    {
        Guard.Against.NegativeOrZero(@event.Duration, nameof(@event.Duration));
        Guard.Against.OutOfRange(@event.Duration, nameof(@event.Duration), minDuration,
            maxDuration);
        if (includeFreeDays != false) return;
        var dateFromDayKind = DateCalculator.GetDayKind(@event.DateFrom);
        if (dateFromDayKind != DateCalculator.DayKind.WORKING)
        {
            throw new ArgumentOutOfRangeException(nameof(@event.DateFrom), "The date from is off work.");
        }

        var dateToDayKind = DateCalculator.GetDayKind(@event.DateTo);
        if (dateToDayKind != DateCalculator.DayKind.WORKING)
        {
            throw new ArgumentOutOfRangeException(nameof(@event.DateTo), "The date to is off work.");
        }
    }
}
