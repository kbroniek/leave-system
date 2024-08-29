namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using System.Net;
using Ardalis.GuardClauses;
using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class BasicValidator(TimeProvider timeProvider, ILeaveTypeFreeDaysRepository leaveTypeRepository)
{
    public virtual async Task<Result<Error>> Validate(
        DateOnly dateFrom,
        DateOnly dateTo,
        TimeSpan? duration,
        Guid leaveTypeId,
        TimeSpan workingHours,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var firstDay = DateOnly.FromDateTime(now.GetFirstDayOfYear().Date);
        var lastDay = DateOnly.FromDateTime(now.GetLastDayOfYear().Date);
        Guard.Against.OutOfRange(dateFrom, nameof(dateFrom), firstDay, lastDay);
        Guard.Against.OutOfRange(dateTo, nameof(dateTo), firstDay, lastDay);
        if (dateFrom > dateTo)
        {
            return new Error("Date from has to be less than date to.", HttpStatusCode.BadRequest);
        }
        var includeFreeDaysResult = await leaveTypeRepository.IsIncludeFreeDays(leaveTypeId, cancellationToken);
        if (!includeFreeDaysResult.IsSuccess)
        {
            return includeFreeDaysResult.Error;
        }
        var maxDuration = DateOnlyCalculator.CalculateDuration(dateFrom, dateTo, workingHours,
            includeFreeDaysResult.Value);
        var minDuration = maxDuration - workingHours + TimeSpan.FromHours(1);
        var durationDefault = duration ?? maxDuration;
        Guard.Against.OutOfRange(durationDefault, nameof(duration), minDuration, maxDuration);
        if (includeFreeDaysResult.Value != false)
        {
            return Result.Default;
        }
        var dateFromDayKind = DateOnlyCalculator.GetDayKind(dateFrom);
        if (dateFromDayKind != DateOnlyCalculator.DayKind.WORKING)
        {
            return new Error("The date from is off work.", HttpStatusCode.BadRequest);
        }

        var dateToDayKind = DateOnlyCalculator.GetDayKind(dateTo);
        if (dateToDayKind != DateOnlyCalculator.DayKind.WORKING)
        {
            return new Error("The date to is off work.", HttpStatusCode.BadRequest);
        }
        return Result.Default;
    }
}

public interface ILeaveTypeFreeDaysRepository
{
    Task<Result<bool?, Error>> IsIncludeFreeDays(Guid leaveTypeId, CancellationToken cancellationToken);
}
