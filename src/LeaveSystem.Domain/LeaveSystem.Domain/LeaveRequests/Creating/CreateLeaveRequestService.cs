using LeaveSystem.Domain.EventSourcing;

namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class CreateLeaveRequestService(WriteRepository repository)
{
    public async Task<Result<LeaveRequest, Error>> CreateAsync(
        Guid leaveRequestId,
        DateOnly dateFrom,
        DateOnly dateTo,
        TimeSpan duration,
        Guid leaveTypeId,
        string? remarks,
        FederatedUser createdBy,
        FederatedUser assignedTo,
        TimeSpan workingHours,
        DateTimeOffset createdDate,
        CancellationToken cancellationToken)
    {
        var leaveRequest = new LeaveRequest();
        var result = leaveRequest.Pending(
            leaveRequestId,
            dateFrom,
            dateTo,
            duration,
            leaveTypeId,
            remarks,
            createdBy,
            assignedTo,
            workingHours,
            createdDate);
        return await result.Match(
            lr => repository.Write(leaveRequest, cancellationToken),
            err => Task.FromResult(Result.Error<LeaveRequest, Error>(err)));

    }
}
