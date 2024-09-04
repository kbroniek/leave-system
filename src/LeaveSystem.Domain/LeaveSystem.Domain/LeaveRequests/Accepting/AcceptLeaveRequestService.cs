namespace LeaveSystem.Domain.LeaveRequests.Accepting;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

public class AcceptLeaveRequestService(ReadService readService, WriteService writeService, CreateLeaveRequestValidator validator)
{
    public async Task<Result<LeaveRequest, Error>> Accept(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken)
    {
        var resultFindById = await readService.FindById<LeaveRequest>(leaveRequestId, cancellationToken);
        if (resultFindById.IsFailure)
        {
            return resultFindById;
        }
        var leaveRequest = resultFindById.Value;
        // Cannot accept overlapping limit or when user doesn't have limits.
        var validateResult = await validator.Validate(
            leaveRequestId, leaveRequest.DateFrom, leaveRequest.DateTo,
            leaveRequest.Duration, leaveRequest.LeaveTypeId, leaveRequest.WorkingHours,
            leaveRequest.AssignedTo.Id, cancellationToken);
        if (validateResult.IsFailure)
        {
            return validateResult.Error;
        }
        var resultAccept = leaveRequest.Accept(leaveRequestId, remarks, acceptedBy, createdDate);
        if (resultAccept.IsFailure)
        {
            return resultAccept;
        }
        return await writeService.Write(resultAccept.Value, cancellationToken);
    }
}
