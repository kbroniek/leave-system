namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using System;
using System.Threading.Tasks;
using LeaveSystem.Shared.Dto;

public interface ILimitValidatorRepository
{
    ValueTask<LeaveLimitDto> GetLimit(DateOnly dateFrom, DateOnly dateTo, Guid leaveTypeId, string userId);
}
