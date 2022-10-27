using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Pages.WorkingHours;

public record class WorkingHoursCollectionDto(IEnumerable<WorkingHoursModel>? WorkingHours);
