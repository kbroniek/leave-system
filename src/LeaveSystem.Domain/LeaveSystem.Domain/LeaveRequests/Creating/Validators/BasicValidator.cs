namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using System.Net;
using Ardalis.GuardClauses;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;

public class BasicValidator(TimeProvider timeProvider)
{
    public virtual Result<Error> DataRangeValidate(LeaveRequestCreated @event)
    {
        var now = timeProvider.GetUtcNow();
        var firstDay = DateOnly.FromDateTime(now.GetFirstDayOfYear().Date);
        var lastDay = DateOnly.FromDateTime(now.GetLastDayOfYear().Date);
        Guard.Against.OutOfRange(@event.DateFrom, nameof(@event.DateFrom), firstDay, lastDay);
        Guard.Against.OutOfRange(@event.DateTo, nameof(@event.DateTo), firstDay, lastDay);
        if (@event.DateFrom > @event.DateTo)
        {
            return new Error("Date from has to be less than date to.", HttpStatusCode.BadRequest);
        }
        return Result.Default;
    }
    public virtual Result<Error> Validate(LeaveRequestCreated @event, TimeSpan minDuration,
        TimeSpan maxDuration, bool? includeFreeDays)
    {
        Guard.Against.NegativeOrZero(@event.Duration);
        Guard.Against.OutOfRange(@event.Duration, nameof(@event.Duration), minDuration,
            maxDuration);
        if (includeFreeDays != false)
            return Result.Ok<Error>();
        var dateFromDayKind = DateOnlyCalculator.GetDayKind(@event.DateFrom);
        if (dateFromDayKind != DateOnlyCalculator.DayKind.WORKING)
        {
            return new Error("The date from is off work.", HttpStatusCode.BadRequest);
        }

        var dateToDayKind = DateOnlyCalculator.GetDayKind(@event.DateTo);
        if (dateToDayKind != DateOnlyCalculator.DayKind.WORKING)
        {
            return new Error("The date to is off work.", HttpStatusCode.BadRequest);
        }
        return Result.Default;
    }
}
