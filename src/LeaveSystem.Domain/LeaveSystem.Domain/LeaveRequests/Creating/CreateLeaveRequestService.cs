namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

public class CreateLeaveRequestService(IReadEventsRepository readEventsRepository, CreateLeaveRequestValidator createLeaveRequestValidator, WriteService writeService)
{
    public async Task<Result<LeaveRequest, Error>> CreateAsync(
        Guid leaveRequestId, DateOnly dateFrom, DateOnly dateTo,
        TimeSpan duration, Guid leaveTypeId, string? remarks,
        LeaveRequestUserDto createdBy, LeaveRequestUserDto assignedTo,
        TimeSpan workingHours, DateTimeOffset createdDate,
        CancellationToken cancellationToken)
    {
        var streamEnumerable = readEventsRepository.ReadStreamAsync(leaveRequestId, cancellationToken).WithCancellation(cancellationToken);
        var enumerator = streamEnumerable.GetAsyncEnumerator();
        // If MoveNextAsync() returns false, the enumerator is empty.
        if (await enumerator.MoveNextAsync())
        {
            return new Error("The resource already exists.", System.Net.HttpStatusCode.Conflict);
        }
        var validateResult = await createLeaveRequestValidator.Validate(
            leaveRequestId, dateFrom, dateTo,
            duration, leaveTypeId, workingHours,
            assignedTo.Id, cancellationToken);
        if (!validateResult.IsSuccess)
        {
            return validateResult.Error;
        }
        var leaveRequest = new LeaveRequest();
        var result = leaveRequest.Pending(
            leaveRequestId, dateFrom, dateTo,
            duration, leaveTypeId, remarks,
            createdBy, assignedTo, workingHours,
            createdDate);
        if (!result.IsSuccess)
        {
            return result;
        }
        return await writeService.Write(result.Value, cancellationToken);
    }
}
