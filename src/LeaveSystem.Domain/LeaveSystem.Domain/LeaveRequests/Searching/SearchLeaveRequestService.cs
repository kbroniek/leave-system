namespace LeaveSystem.Domain.LeaveRequests.Searching;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;

public class SearchLeaveRequestService(ISearchLeaveRequestRepository searchLeaveRequestRepository, ReadService readService, TimeProvider timeProvider)
{
    public async Task<(IEnumerable<SearchLeaveRequestsResultDto> results, SearchLeaveRequestsQueryDto search)> Search(string? continuationToken, DateOnly? dateFrom, DateOnly? dateTo,
        Guid[]? leaveTypeIds, LeaveRequestStatus[]? statuses, string[]? assignedToUserIds, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var statusesOrDefault = statuses ?? [LeaveRequestStatus.Pending, LeaveRequestStatus.Accepted];
        var dateFromOrDefault = dateFrom ?? GetMinDate(now);
        var dateToOrDefault = dateTo ?? GetMaxDate(now);
        (var pendingEvents, var continuationTokenResult) = await searchLeaveRequestRepository.GetPendingEvents(
            continuationToken, dateFromOrDefault, dateToOrDefault,
            leaveTypeIds ?? [], assignedToUserIds ?? [], cancellationToken);
        var leaveRequests = await readService.FindByIds<LeaveRequest>(pendingEvents.Select(x => x.LeaveRequestId).ToArray(), cancellationToken);
        var searchedLeaveRequests = leaveRequests
            .Where(x => statusesOrDefault.Contains(x.Status))
            .Select(x => new SearchLeaveRequestsResultDto(x.Id, x.DateFrom, x.DateTo, x.Duration, x.LeaveTypeId, x.Status, x.CreatedBy.Name, x.WorkingHours))
            .ToList();
        var search = new SearchLeaveRequestsQueryDto(
            continuationTokenResult, dateFromOrDefault, dateToOrDefault,
            leaveTypeIds, statusesOrDefault, assignedToUserIds);
        return (searchedLeaveRequests, search);
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
        Guid[] leaveTypeIds, string[] assignedToUserIds, CancellationToken cancellationToken);

    public sealed record PendingEventEntity(Guid LeaveRequestId, Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    public sealed record EventUserEntity(string Id);
}
