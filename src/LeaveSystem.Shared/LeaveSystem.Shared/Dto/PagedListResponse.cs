namespace LeaveSystem.Shared.Dto;

public static class PagedListResponseExtensions
{
    public static PagedListResponse<T> ToPagedListResponse<T>(this IEnumerable<T>? items, string? continuationToken = null) => new(items, continuationToken);
}

public record PagedListResponse<T>(IEnumerable<T>? Items, string? ContinuationToken = null);
