namespace LeaveSystem.Web.UnitTests.TestStuff.Extensions;

public static class EnumerableExtensions
{
    public static PagedListResponse<T> ToPagedListResponse<T>(this IEnumerable<T> source, int totalItemCount)
    {
        return new PagedListResponse<T>(source, totalItemCount, false);
    }
}