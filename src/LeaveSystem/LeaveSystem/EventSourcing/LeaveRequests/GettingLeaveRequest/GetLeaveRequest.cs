using Ardalis.GuardClauses;
using GoldenEye.Queries;
using GoldenEye.Repositories;
using LeaveSystem.Shared;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequest;

public class GetLeaveRequest : IQuery<LeaveRequest>
{
    public Guid LeaveRequestId { get; }

    private GetLeaveRequest(Guid leaveRequestId)
    {
        LeaveRequestId = leaveRequestId;
    }

    public static GetLeaveRequest Create(Guid? leaveRequestId)
    {
        return new(Guard.Against.Nill(leaveRequestId));
    }
}


internal class HandleGetLeaveRequest :
    IQueryHandler<GetLeaveRequest, LeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleGetLeaveRequest(IRepository<LeaveRequest> repository)
    {
        this.repository = repository;
    }

    public async Task<LeaveRequest> Handle(GetLeaveRequest request,
        CancellationToken cancellationToken)
    {
        LeaveRequest leaveRequest = await repository.FindById(request.LeaveRequestId, cancellationToken);
        return leaveRequest
            ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(request.LeaveRequestId);
    }
}
