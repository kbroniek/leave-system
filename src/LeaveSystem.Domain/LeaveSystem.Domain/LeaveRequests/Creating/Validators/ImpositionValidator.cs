namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;

public class ImpositionValidator(IImpositionValidatorRepository impositionValidatorRepository)
{
    public virtual async Task<Result<Error>> Validate(LeaveRequestCreated creatingLeaveRequest)
    {
        if (await impositionValidatorRepository.ExistValid(creatingLeaveRequest.CreatedBy.Id, creatingLeaveRequest.DateFrom, creatingLeaveRequest.DateTo))
        {
            return new Error("Cannot create a new leave request in this time. The other leave is overlapping with this date", System.Net.HttpStatusCode.BadRequest);
        }
        return Result.Ok<Error>();
    }
}
public interface IImpositionValidatorRepository
{
    ValueTask<bool> ExistValid(string createdById, DateOnly dateFrom, DateOnly dateTo);
}
