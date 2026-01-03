namespace LeaveSystem.Functions.LeaveLimits.Repositories;

using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.WorkingHours;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

public class SearchLeaveLimitsRepository(CosmosClient cosmosClient, string databaseName, string containerId, ILogger<SearchLeaveLimitsRepository> logger)
{
    private const int MaxPageSize = 1000;

    public async Task<Result<(IReadOnlyList<LeaveLimitDto> limits, string? continuationToken), Error>> GetLimits(
        int year, string[] assignedToUserIds, Guid[] leaveTypeIds,
        int? pageSize, string? continuationToken, CancellationToken cancellationToken)
    {
        var firstDay = new DateOnly(year, 1, 1);
        var lastDay = new DateOnly(year, 12, 31);
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var pageSizeOrMax = pageSize < MaxPageSize ? pageSize ?? MaxPageSize : MaxPageSize;
        var result = await container.GetItemLinqQueryable<LeaveLimitDto>(continuationToken: continuationToken, requestOptions: new QueryRequestOptions { MaxItemCount = pageSizeOrMax })
            .Where(x => (leaveTypeIds.Length == 0 || leaveTypeIds.Contains(x.LeaveTypeId)) &&
                (!x.AssignedToUserId.IsDefined() || x.AssignedToUserId.IsNull() || assignedToUserIds.Length == 0 || assignedToUserIds.Contains(x.AssignedToUserId)) &&
                (!x.ValidSince.IsDefined() || x.ValidSince.IsNull() || x.ValidSince >= firstDay) &&
                (!x.ValidUntil.IsDefined() || x.ValidUntil.IsNull() || x.ValidUntil <= lastDay))
            .ToFeedIterator()
            .ExecuteQuery(logger, pageSizeOrMax, cancellationToken);
        return result;
    }

    public async Task<Result<IReadOnlyList<LeaveLimitDto>, Error>> GetLimitTemplatesForNewYear(
        int templateYear, Guid[] saturdayLeaveTypeIds, CancellationToken cancellationToken)
    {
        var firstDay = new DateOnly(templateYear, 1, 1);
        var lastDay = new DateOnly(templateYear, 12, 31);
        var container = cosmosClient.GetContainer(databaseName, containerId);

        var templates = new List<LeaveLimitDto>();
        var continuationToken = (string?)null;
        var pageSize = 1000;

        do
        {
            var result = await container.GetItemLinqQueryable<LeaveLimitDto>(continuationToken: continuationToken, requestOptions: new QueryRequestOptions { MaxItemCount = pageSize })
                .Where(x =>
                    (saturdayLeaveTypeIds.Length == 0 || !saturdayLeaveTypeIds.Contains(x.LeaveTypeId)) &&
                    x.ValidSince <= lastDay &&
                   x.ValidUntil >= firstDay)
                .ToFeedIterator()
                .ExecuteQuery(logger, pageSize, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error;
            }

            templates.AddRange(result.Value.results.Select(x => x with { OverdueLimit = null }));
            continuationToken = result.Value.continuationToken;
        }
        while (continuationToken != null);

        if (saturdayLeaveTypeIds.Length > 0)
        {
            AddSaturdayLimit(templates, firstDay, lastDay, saturdayLeaveTypeIds[0]);
        }

        return templates;
    }

    public void AddSaturdayLimit(
        List<LeaveLimitDto> nextYearLimits,
        DateOnly firstDayOfYear,
        DateOnly lastDayOfYear,
        Guid saturdayLeaveTypeId)
    {
        // Find the first Saturday in the year
        var saturday = firstDayOfYear;
        while (saturday <= lastDayOfYear && saturday.DayOfWeek != DayOfWeek.Saturday)
        {
            saturday = saturday.AddDays(1);
        }

        // Get all unique user IDs from the existing limits
        var userIds = nextYearLimits
            .Where(x => !string.IsNullOrEmpty(x.AssignedToUserId))
            .Select(x => x.AssignedToUserId!)
            .Distinct()
            .ToList();

        // Iterate through all Saturdays in the year
        while (saturday <= lastDayOfYear)
        {
            // Check if Saturday is a holiday
            if (DateOnlyCalculator.GetDayKind(saturday) == DateOnlyCalculator.DayKind.HOLIDAY)
            {
                // Add Saturday limit for each user
                foreach (var userId in userIds)
                {
                    AddSaturdayLimitPerUser(userId, nextYearLimits, saturday, saturdayLeaveTypeId);
                }
            }
            saturday = saturday.AddDays(7); // Move to next Saturday
        }
    }

    private static void AddSaturdayLimitPerUser(
        string userId,
        List<LeaveLimitDto> nextYearLimits,
        DateOnly saturday,
        Guid saturdayLeaveTypeId)
    {
        var currentYear = saturday.Year;
        var currentMonth = saturday.Month;
        var firstDayOfMonth = new DateOnly(currentYear, currentMonth, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var saturdayLimit = new LeaveLimitDto(
            Id: Guid.NewGuid(),
            Limit: TimeSpan.FromHours(8), // 1 day = 8 hours
            OverdueLimit: null,
            WorkingHours: WorkingHoursUtils.DefaultWorkingHours,
            LeaveTypeId: saturdayLeaveTypeId,
            ValidSince: firstDayOfMonth,
            ValidUntil: lastDayOfMonth,
            AssignedToUserId: userId,
            State: LeaveLimitDto.LeaveLimitState.Active,
            Description: saturday.ToString("yyyy-MM-dd"));

        nextYearLimits.Add(saturdayLimit);
    }

}
