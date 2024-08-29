using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class ImpositionValidator(IImpositionValidatorRepository impositionValidatorRepository)
{
    public virtual async Task<Result<Error>> Validate(LeaveRequestCreated creatingLeaveRequest)
    {
        if (await impositionValidatorRepository.ExistValid(creatingLeaveRequest.CreatedBy.Id, creatingLeaveRequest.DateFrom, creatingLeaveRequest.DateTo))
        {
            return new Error("Cannot create a new leave request in this time. The other leave is overlapping with this date");
        }
        return Result.Ok<Error>();
    }
}
