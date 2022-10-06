using System.Text.Json.Serialization;

namespace LeaveSystem.Web;

public class PagedListResponse<T>
{
    public IEnumerable<T>? Items { get; set; }

    public long TotalItemCount { get; set; }

    public bool HasNextPage { get; set; }

    //[JsonConstructor]
    //public PagedListResponse()
    //{
    //    Items = new List<T>();
    //    TotalItemCount = 0;
    //    HasNextPage = false;
    //}
    //public PagedListResponse(IEnumerable<T> items, long totalItemCount, bool hasNextPage)
    //{
    //    Items = items.ToList();
    //    TotalItemCount = totalItemCount;
    //    HasNextPage = hasNextPage;
    //}
}