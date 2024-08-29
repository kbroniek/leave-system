namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;
public interface IImpositionValidatorRepository
{
    ValueTask<bool> ExistValid(string createdById, DateOnly dateFrom, DateOnly dateTo);
}
