namespace LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;

public class AcceptLeaveRequestService(ReadService readService, WriteService writeService)
{
    public async Task<Result<LeaveRequest, Error>> AcceptAsync(Guid leaveRequestId, string? remarks, LeaveRequestUserDto acceptedBy, DateTimeOffset createdDate, CancellationToken cancellationToken)
    {
        var resultFindById = await readService.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken);
        if (!resultFindById.IsSuccess)
        {
            return resultFindById;
        }
        var resultAccept = resultFindById.Value.Accept(leaveRequestId, remarks, acceptedBy, createdDate);
        if (!resultAccept.IsSuccess)
        {
            return resultAccept;
        }
        return await writeService.Write(resultAccept.Value, cancellationToken);
    }
}
