namespace LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;

public class CreateLeaveRequestService(CreateLeaveRequestValidator createLeaveRequestValidator, WriteService writeService)
{
    public async Task<Result<LeaveRequest, Error>> CreateAsync(
        Guid leaveRequestId, DateOnly dateFrom, DateOnly dateTo,
        TimeSpan duration, Guid leaveTypeId, string? remarks,
        LeaveRequestUserDto createdBy, LeaveRequestUserDto assignedTo,
        TimeSpan workingHours, DateTimeOffset createdDate,
        CancellationToken cancellationToken)
    {
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
