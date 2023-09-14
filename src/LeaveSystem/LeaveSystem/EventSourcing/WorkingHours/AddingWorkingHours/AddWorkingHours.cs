using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using Marten;
using MediatR;

namespace LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;

public class AddWorkingHours : ICommand
{
    public Guid WorkingHoursId { get; }
    public string UserId { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset? DateTo { get; }
    public TimeSpan Duration { get; }
    public FederatedUser AddedBy { get; }

    private AddWorkingHours(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo,
        TimeSpan duration, FederatedUser addedBy)
    {
        UserId = userId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        AddedBy = addedBy;
        WorkingHoursId = workingHoursId;
    }

    public static AddWorkingHours Create(Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo, TimeSpan? duration, FederatedUser? addedBy) =>
        new(
            Guard.Against.NillAndDefault(workingHoursId),
            Guard.Against.NullOrWhiteSpace(userId),
            Guard.Against.NillAndDefault(dateFrom),
            dateTo,
            Guard.Against.NillAndDefault(duration),
            Guard.Against.NillAndDefault(addedBy)
        );
}

internal class HandleAddWorkingHours : ICommandHandler<AddWorkingHours>
{
    private readonly IRepository<WorkingHours> workingHoursRepository;
    private readonly IRepository<LeaveRequest> leaveRequestRepository;
    private readonly WorkingHoursFactory factory;
    private readonly IDocumentSession querySession;

    public HandleAddWorkingHours(IRepository<WorkingHours> workingHoursRepository, WorkingHoursFactory factory,
        IDocumentSession querySession, IRepository<LeaveRequest> leaveRequestRepository)
    {
        this.workingHoursRepository = workingHoursRepository;
        this.factory = factory;
        this.querySession = querySession;
        this.leaveRequestRepository = leaveRequestRepository;
    }

    public async Task<Unit> Handle(AddWorkingHours request, CancellationToken cancellationToken)
    {
        if (!request.AddedBy.CanManageWorkingHours())
        {
            throw new InvalidOperationException("You have no privileges to add working hours");
        }
        var currentWorkingHoursForUser = await querySession.Query<WorkingHours>()
            .Where(x => x.UserId == request.UserId && x.Status == WorkingHoursStatus.Current)
            .FirstOrDefaultAsync(cancellationToken);
        if (currentWorkingHoursForUser is not null)
        {
            currentWorkingHoursForUser.Deprecate();
            await workingHoursRepository.Update(currentWorkingHoursForUser, cancellationToken);
            await workingHoursRepository.SaveChanges(cancellationToken);
            await RejectUserLeaveRequests(request.UserId, request.AddedBy, cancellationToken);
        }
        var createdWorkingHours = factory.Create(request);
        await workingHoursRepository.Add(createdWorkingHours, cancellationToken);
        await workingHoursRepository.SaveChanges(cancellationToken);
        return Unit.Value;
    }

    private async Task RejectUserLeaveRequests(string userId, FederatedUser canceledBy, CancellationToken cancellationToken)
    {
        var validLeaveRequestsCreatedByUser = await querySession.Query<LeaveRequest>()
            .Where(x => x.CreatedBy.Id == userId && x.Status.IsValid()).ToListAsync(token: cancellationToken);
        foreach (var leaveRequest in validLeaveRequestsCreatedByUser)
        {
            leaveRequest.Reject("canceled because of changed working hours", canceledBy);
            await leaveRequestRepository.Update(leaveRequest, cancellationToken);
            await leaveRequestRepository.SaveChanges(cancellationToken);
        }
    }
    
}