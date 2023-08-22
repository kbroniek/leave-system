using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Pages.WorkingHours;

public record WorkingHoursCollectionDto(IEnumerable<WorkingHoursModel>? WorkingHours);
