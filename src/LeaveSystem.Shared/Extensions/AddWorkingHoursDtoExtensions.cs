using Ardalis.GuardClauses;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Shared.Extensions;

public static class AddWorkingHoursDtoExtensions
{
    public static WorkingHoursDto ToWorkingHoursDto(this AddWorkingHoursDto source, Guid id) =>
        new(
            Guard.Against.NullOrWhiteSpace(source.UserId),
            Guard.Against.NillAndDefault(source.DateFrom),
            source.DateTo,
            Guard.Against.NillAndDefault(source.Duration),
            id
            );
}