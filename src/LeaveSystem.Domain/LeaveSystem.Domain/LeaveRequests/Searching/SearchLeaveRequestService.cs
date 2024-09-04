namespace LeaveSystem.Domain.LeaveRequests.Searching;
using System;
using System.Threading.Tasks;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;

public class SearchLeaveRequestService(ISearchLeaveRequestRepository searchLeaveRequestRepository, TimeProvider timeProvider)
{
    public async Task<(IEnumerable<SearchLeaveRequestsResultDto> results, string? continuationToken)> Search(string? continuationToken, DateOnly? dateFrom, DateOnly? dateTo,
        Guid[]? leaveTypeIds, LeaveRequestStatus[]? statuses, string[]? assignedToUserIds, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var statusesOrDefault = statuses ?? [LeaveRequestStatus.Pending, LeaveRequestStatus.Accepted];
        return await searchLeaveRequestRepository.Search(
            continuationToken, dateFrom ?? GetMinDate(now), dateTo ?? GetMaxDate(now),
            leaveTypeIds, statusesOrDefault, assignedToUserIds, cancellationToken);
    }

    private static DateOnly GetMinDate(DateTimeOffset now) =>
        DateOnly.FromDateTime(now.AddDays(-14).Date);

    private static DateOnly GetMaxDate(DateTimeOffset now) =>
        DateOnly.FromDateTime(now.AddDays(14).Date);
}

public interface ISearchLeaveRequestRepository
{
    Task<(IEnumerable<SearchLeaveRequestsResultDto> results, string? continuationToken)> Search(
        string? continuationToken, DateOnly dateFrom, DateOnly dateTo,
        Guid[]? leaveTypeIds, LeaveRequestStatus[] statuses, string[]? assignedToUserIds, CancellationToken cancellationToken);
}
