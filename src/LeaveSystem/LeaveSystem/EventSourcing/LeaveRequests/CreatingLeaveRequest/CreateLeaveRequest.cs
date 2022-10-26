using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequest : ICommand
{
    public Guid LeaveRequestId { get; }

    public DateTimeOffset DateFrom { get; }

    public DateTimeOffset DateTo { get; }

    public TimeSpan? Duration { get; }

    public Guid LeaveTypeId { get; }

    public string? Remarks { get; }

    public FederatedUser CreatedBy { get; }

    public TimeSpan WorkingHours { get; }

    private CreateLeaveRequest(Guid leaveRequestId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan? duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy, TimeSpan workingHours)
    {
        LeaveRequestId = leaveRequestId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        LeaveTypeId = leaveTypeId;
        Remarks = remarks;
        CreatedBy = createdBy;
        WorkingHours = workingHours;
    }
    public static CreateLeaveRequest Create(Guid? leaveRequestId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, TimeSpan? duration, Guid? leaveTypeId, string? remarks, FederatedUser? createdBy, TimeSpan? workingHours)
    {
        return new(
            Guard.Against.NillAndDefault(leaveRequestId),
            Guard.Against.NillAndDefault(dateFrom),
            Guard.Against.NillAndDefault(dateTo),
            duration,
            Guard.Against.NillAndDefault(leaveTypeId),
            remarks,
            Guard.Against.NillAndDefault(createdBy),
            Guard.Against.NillAndDefault(workingHours));
    }
}

internal class HandleCreateLeaveRequest :
    ICommandHandler<CreateLeaveRequest>
{
    private readonly IRepository<LeaveRequest> repository;
    private readonly LeaveRequestFactory leaveRequestFactory;

    public HandleCreateLeaveRequest(IRepository<LeaveRequest> repository, LeaveRequestFactory leaveRequestFactory)
    {
        this.repository = repository;
        this.leaveRequestFactory = leaveRequestFactory;
    }

    public async Task<Unit> Handle(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestFactory.Create(command);
        await repository.Add(leaveRequest, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}