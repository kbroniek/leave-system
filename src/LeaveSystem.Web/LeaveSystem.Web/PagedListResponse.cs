namespace LeaveSystem.Web;

public record class PagedListResponse<T>(IEnumerable<T>? Items, long TotalItemCount, bool HasNextPage);