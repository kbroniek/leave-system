namespace LeaveSystem.Shared.Dto;

public static class PagedListResponseExtensions
{
    public static PagedListResponse<T> ToPagedListResponse<T>(this IEnumerable<T>? items) => new(items, items.LongCount(), false);
    public static PagedListResponse<T> ToPagedListResponse<T>(this IEnumerable<T>? items, long totalItemCount, bool hasNextPage) => new(items, totalItemCount, hasNextPage);
}

public record PagedListResponse<T>(IEnumerable<T>? Items, long TotalItemCount, bool HasNextPage);
