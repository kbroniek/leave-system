using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Queries;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.Shared;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;

public class GetLeaveRequestDetails : IQuery<LeaveRequest>
{
    public Guid LeaveRequestId { get; }

    private GetLeaveRequestDetails(Guid leaveRequestId) =>
        LeaveRequestId = leaveRequestId;

    public static GetLeaveRequestDetails Create(Guid? leaveRequestId) =>
        new(Guard.Against.Nill(leaveRequestId));
}


internal class HandleGetLeaveRequestDetails :
    IQueryHandler<GetLeaveRequestDetails, LeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleGetLeaveRequestDetails(IRepository<LeaveRequest> repository) =>
        this.repository = repository;

    public async Task<LeaveRequest> Handle(GetLeaveRequestDetails request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindByIdAsync(request.LeaveRequestId, cancellationToken);
        return leaveRequest
            ?? throw GoldenEye.Backend.Core.Exceptions.NotFoundException.For<LeaveRequest>(request.LeaveRequestId);
    }
}
