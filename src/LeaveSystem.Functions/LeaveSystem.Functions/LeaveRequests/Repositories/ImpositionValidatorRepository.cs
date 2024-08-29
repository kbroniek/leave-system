namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;

internal class ImpositionValidatorRepository : IImpositionValidatorRepository
{
    public ValueTask<bool> ExistValid(string createdById, DateOnly dateFrom, DateOnly dateTo) => throw new NotImplementedException();
}
