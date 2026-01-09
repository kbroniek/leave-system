namespace LeaveSystem.Domain.LeaveRequests.Creating;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Shared;

public interface IDecisionMakerRepository
{
    Task<Result<IReadOnlyCollection<string>, Error>> GetDecisionMakerUserIds(CancellationToken cancellationToken);
}
