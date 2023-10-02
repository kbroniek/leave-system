using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.DeprecatingLeaveRequest;

public class DeprecateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }
    public string? Remarks { get; }
    public FederatedUser DeprecatedBy { get; }

    private DeprecateLeaveRequest(Guid leaveRequestId, string? remarks, FederatedUser deprecatedBy)
    {
        DeprecatedBy = deprecatedBy;
        Remarks = remarks;
        LeaveRequestId = leaveRequestId;
    }

    public static DeprecateLeaveRequest Create(Guid? leaveRequestId, string? remarks, FederatedUser? deprecatedBy)
    {
        var deprecatedByNotNull = Guard.Against.Nill(deprecatedBy);
        Guard.Against.InvalidEmail(deprecatedByNotNull.Email, $"{nameof(deprecatedByNotNull)}.{nameof(deprecatedByNotNull.Email)}");
        return new (
            Guard.Against.NillAndDefault(leaveRequestId),
            remarks,
            deprecatedByNotNull
        );
    }
}

internal class HandleDeprecateLeaveRequest : ICommandHandler<DeprecateLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;

    public HandleDeprecateLeaveRequest(IRepository<LeaveRequest> repository)
    {
        this.repository = repository;
    }

    public async Task<Unit> Handle(DeprecateLeaveRequest request, CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.FindById(request.LeaveRequestId, cancellationToken)
            ?? throw GoldenEye.Exceptions.NotFoundException.For<LeaveRequest>(request.LeaveRequestId);
        leaveRequest.Deprecate(request.Remarks, request.DeprecatedBy);
        await repository.Update(leaveRequest, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}
