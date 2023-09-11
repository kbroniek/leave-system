namespace LeaveSystem.Web;

public record PagedListResponse<T>(IEnumerable<T>? Items, long TotalItemCount, bool HasNextPage);