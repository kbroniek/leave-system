namespace LeaveSystem.Dal.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;

internal static class CosmosDbExtensions
{
    public static async Task<IList<T>> ToListAsync<T>(this IQueryable<T> query)
    {
        var items = new List<T>();
        using var iterator = query.ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            items.AddRange(await iterator.ReadNextAsync());
        }
        return items;
    }
}
