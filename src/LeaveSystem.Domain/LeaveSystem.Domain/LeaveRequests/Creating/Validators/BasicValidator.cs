namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using System.Net;
using Ardalis.GuardClauses;
using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class BasicValidator(ILeaveTypeFreeDaysRepository leaveTypeRepository)
{
    public virtual async Task<Result<Error>> Validate(
        DateOnly dateFrom,
        DateOnly dateTo,
        TimeSpan? duration,
        Guid leaveTypeId,
        TimeSpan workingHours,
        CancellationToken cancellationToken)
    {
        if (dateFrom.Year != dateTo.Year)
        {
            return new Error("Date from and date to must be in the same year.", HttpStatusCode.BadRequest, ErrorCodes.INVALID_DATE_RANGE);
        }
        if (dateFrom > dateTo)
        {
            return new Error("Date from has to be less than date to.", HttpStatusCode.BadRequest, ErrorCodes.INVALID_DATE_RANGE);
        }

        var includeFreeDaysResult = await leaveTypeRepository.IsIncludeFreeDays(leaveTypeId, cancellationToken);
        if (!includeFreeDaysResult.IsSuccess)
        {
            return includeFreeDaysResult.Error;
        }

        if (includeFreeDaysResult.Value == false)
        {
            var dateFromDayKind = DateOnlyCalculator.GetDayKind(dateFrom);
            if (dateFromDayKind != DateOnlyCalculator.DayKind.WORKDAY)
            {
                return new Error("The date from is off work.", HttpStatusCode.BadRequest, ErrorCodes.OFF_WORK_DATE);
            }

            var dateToDayKind = DateOnlyCalculator.GetDayKind(dateTo);
            if (dateToDayKind != DateOnlyCalculator.DayKind.WORKDAY)
            {
                return new Error("The date to is off work.", HttpStatusCode.BadRequest, ErrorCodes.OFF_WORK_DATE);
            }
        }

        var maxDuration = DateOnlyCalculator.CalculateDuration(
            dateFrom, dateTo, workingHours, includeFreeDaysResult.Value);
        var minDuration = maxDuration - workingHours + TimeSpan.FromHours(1);
        var durationDefault = duration ?? maxDuration;
        Guard.Against.OutOfRange(durationDefault, nameof(duration), minDuration, maxDuration);
        return Result.Default;
    }
}

public interface ILeaveTypeFreeDaysRepository
{
    public Task<Result<bool?, Error>> IsIncludeFreeDays(Guid leaveTypeId, CancellationToken cancellationToken);
}
