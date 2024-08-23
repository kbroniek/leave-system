using LeaveSystem.Domain.EventSourcing;

namespace LeaveSystem.Domain.LeaveRequests.Accepting;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class AcceptLeaveRequestService(ReadRepository readRepository, WriteRepository writeRepository)
{
    public async Task<Result<LeaveRequest, Error>> AcceptAsync(Guid leaveRequestId, string? remarks, FederatedUser acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken)
    {
        var leaveRequest = await readRepository.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken);
        var result = leaveRequest.Accept(leaveRequestId, remarks, acceptedBy, createdDate);
        return await result.Match(
            lr => writeRepository.Write(leaveRequest, cancellationToken),
            err => Task.FromResult(Result.Error<LeaveRequest, Error>(err)));
    }
}
