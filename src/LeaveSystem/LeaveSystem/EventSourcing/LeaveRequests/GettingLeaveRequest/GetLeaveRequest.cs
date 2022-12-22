using GoldenEye.Queries;
using GoldenEye.Repositories;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequest;

public class GetLeaveRequest : IQuery<LeaveRequest>
{
    public Guid LeaveRequestId { get; }

    private GetLeaveRequest(Guid leaveRequestId)
    {
        LeaveRequestId = leaveRequestId;
    }

    public static GetLeaveRequest Create(Guid leaveRequestId)
    {
        return new(leaveRequestId);
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
        return await repository.FindById(request.LeaveRequestId, cancellationToken)
            ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(request.LeaveRequestId);
    }
}
