namespace LeaveSystem.Functions.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

internal static class CosmosExtensions
{
    internal static async Task<IReadOnlyCollection<T>> ExecuteQuery<T>(this FeedIterator<T> iterator, CancellationToken cancellationToken)
    {
        using (iterator)
        {
            List<T> results = [];
            while (iterator.HasMoreResults)
            {
                var queryResult = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(queryResult);
            }

            return results;
        }
    }
}
