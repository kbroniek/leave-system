namespace LeaveSystem.Shared.Dto;

public record PagedListResponse<T>(IEnumerable<T>? Items, long TotalItemCount, bool HasNextPage);
