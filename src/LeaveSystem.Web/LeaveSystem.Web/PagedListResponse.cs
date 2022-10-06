using System.Text.Json.Serialization;

namespace LeaveSystem.Web;

public class PagedListResponse<T>
{
    public IReadOnlyList<T> Items { get; }

    public long TotalItemCount { get; }

    public bool HasNextPage { get; }

    [JsonConstructor]
    public PagedListResponse(IEnumerable<T> items, long totalItemCount, bool hasNextPage)
    {
        Items = items.ToList();
        TotalItemCount = totalItemCount;
        HasNextPage = hasNextPage;
    }
}