using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class EnumerableExtensions
{
    public static PagedListResponse<T> ToPagedListResponse<T>(this IEnumerable<T> source) => new PagedListResponse<T>(source, source.Count(), false);
    public static IEnumerable<WorkingHoursDto> ToDto(this IEnumerable<WorkingHours> source) => source.Select(x => new WorkingHoursDto(x.UserId, x.DateFrom, x.DateTo, x.Duration, x.Status));
}