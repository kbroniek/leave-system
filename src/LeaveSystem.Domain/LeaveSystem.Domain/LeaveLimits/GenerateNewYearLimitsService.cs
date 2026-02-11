namespace LeaveSystem.Domain.LeaveLimits;

using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.WorkingHours;
using Microsoft.Extensions.Logging;

public class GenerateNewYearLimitsService(ILogger<GenerateNewYearLimitsService>? logger)
{
    public List<LeaveLimitDto> GenerateNewYearLimits(
        IEnumerable<LeaveLimitDto> templateLimits,
        IEnumerable<LeaveRequestEventDto> pendingEvents,
        IReadOnlyCollection<Guid> cancelledStreamIds,
        int templateYear,
        Guid? saturdayLeaveTypeId)
    {
        var newYear = templateYear + 1;
        var firstDayOfTemplateYear = new DateOnly(templateYear, 1, 1);
        var lastDayOfTemplateYear = new DateOnly(templateYear, 12, 31);
        var firstDayOfNewYear = new DateOnly(newYear, 1, 1);
        var lastDayOfNewYear = new DateOnly(newYear, 12, 31);

        var templates = templateLimits.ToList();

        // Calculate overdue limits based on unused leave from the previous year
        CalculateOverdueLimits(templates, pendingEvents, cancelledStreamIds, firstDayOfTemplateYear, lastDayOfTemplateYear);

        // Transform templates to new year limits
        var newYearLimits = TransformToNewYearLimits(templates, newYear);

        // Add Saturday limits if Saturday leave type is provided
        if (saturdayLeaveTypeId.HasValue)
        {
            AddSaturdayLimits(newYearLimits, firstDayOfNewYear, lastDayOfNewYear, saturdayLeaveTypeId.Value);
        }

        return newYearLimits;
    }

    private void CalculateOverdueLimits(
        List<LeaveLimitDto> templates,
        IEnumerable<LeaveRequestEventDto> pendingEvents,
        IReadOnlyCollection<Guid> cancelledStreamIds,
        DateOnly firstDayOfTemplateYear,
        DateOnly lastDayOfTemplateYear)
    {
        // Filter out cancelled events
        var activeEvents = pendingEvents
            .Where(e => !cancelledStreamIds.Contains(e.StreamId))
            .ToList();

        // Use for loop with index to avoid collection modification during enumeration
        for (var i = 0; i < templates.Count; i++)
        {
            var template = templates[i];

            // Skip if no user assigned (global limits don't have overdue)
            if (string.IsNullOrEmpty(template.AssignedToUserId))
            {
                continue;
            }

            // Skip if no limit set
            if (template.Limit == null)
            {
                continue;
            }

            try
            {
                // Calculate used leaves duration for this user and leave type in the template year
                var validSince = template.ValidSince ?? firstDayOfTemplateYear;
                var validUntil = template.ValidUntil ?? lastDayOfTemplateYear;

                var usedDuration = activeEvents
                    .Where(e =>
                        e.AssignedToUserId == template.AssignedToUserId &&
                        e.LeaveTypeId == template.LeaveTypeId &&
                        e.DateFrom >= validSince &&
                        e.DateTo <= validUntil)
                    .Sum(e => e.Duration.Ticks);

                // Calculate overdue limit: what's left from the limit that wasn't used
                // overdueLimit = max(0, limit - used)
                var limitValue = template.Limit.Value;
                var remainingLimit = limitValue - TimeSpan.FromTicks(usedDuration);
                var overdueLimit = remainingLimit > TimeSpan.Zero ? remainingLimit : TimeSpan.Zero;

                // Update the template with calculated overdue limit
                templates[i] = template with { OverdueLimit = overdueLimit };
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex,
                    "Failed to calculate overdue limit for limit {LimitId}, user {UserId}, leaveType {LeaveTypeId}",
                    template.Id, template.AssignedToUserId, template.LeaveTypeId);
                // Continue with null overdue limit if calculation fails
            }
        }
    }

    private static List<LeaveLimitDto> TransformToNewYearLimits(List<LeaveLimitDto> templates, int newYear)
    {
        var newYearLimits = new List<LeaveLimitDto>();

        foreach (var template in templates)
        {
            var newLimit = template with
            {
                Id = Guid.NewGuid(),
                ValidSince = template.ValidSince.HasValue
                    ? new DateOnly(newYear, template.ValidSince.Value.Month, template.ValidSince.Value.Day)
                    : new DateOnly(newYear, 1, 1),
                ValidUntil = template.ValidUntil.HasValue
                    ? new DateOnly(newYear, template.ValidUntil.Value.Month, template.ValidUntil.Value.Day)
                    : new DateOnly(newYear, 12, 31)
            };
            newYearLimits.Add(newLimit);
        }

        return newYearLimits;
    }

    private void AddSaturdayLimits(
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
