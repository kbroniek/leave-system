namespace LeaveSystem.Domain.LeaveRequests.Searching;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;

public class SearchLeaveRequestService(ISearchLeaveRequestRepository searchLeaveRequestRepository, ReadService readService, TimeProvider timeProvider)
{
    public async Task<(IEnumerable<SearchLeaveRequestsResultDto> results, string? continuationToken)> Search(string? continuationToken, DateOnly? dateFrom, DateOnly? dateTo,
        Guid[]? leaveTypeIds, LeaveRequestStatus[]? statuses, string[]? assignedToUserIds, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var statusesOrDefault = statuses ?? [LeaveRequestStatus.Pending, LeaveRequestStatus.Accepted];
        (var pendingEvents, var continuationTokenResult) = await searchLeaveRequestRepository.GetPendingEvents(
            continuationToken, dateFrom ?? GetMinDate(now), dateTo ?? GetMaxDate(now),
            leaveTypeIds, statusesOrDefault, assignedToUserIds, cancellationToken);
        var leaveRequests = await readService.FindByIds<LeaveRequest>(pendingEvents.Select(x => x.LeaveRequestId).ToArray(), cancellationToken);
    }

    private static DateOnly GetMinDate(DateTimeOffset now) =>
        DateOnly.FromDateTime(now.AddDays(-14).Date);

    private static DateOnly GetMaxDate(DateTimeOffset now) =>
        DateOnly.FromDateTime(now.AddDays(14).Date);
}

public interface ISearchLeaveRequestRepository
{
    Task<(IEnumerable<PendingEventEntity> pendingEvents, string? continuationToken)> GetPendingEvents(
        string? continuationToken, DateOnly dateFrom, DateOnly dateTo,
        Guid[]? leaveTypeIds, LeaveRequestStatus[] statuses, string[]? assignedToUserIds, CancellationToken cancellationToken);

    public sealed record PendingEventEntity(Guid LeaveRequestId, Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    public sealed record EventUserEntity(string Id);
}
