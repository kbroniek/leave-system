namespace LeaveSystem.Functions.Extensions;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

internal static class CosmosExtensions
{
    [Obsolete("Use version with error handling")]
    internal static async Task<IReadOnlyList<T>> ExecuteQuery<T>(this FeedIterator<T> iterator, CancellationToken cancellationToken)
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
    internal static async Task<Result<IReadOnlyList<T>, Error>> ExecuteQuery<T>(this FeedIterator<T> iterator, ILogger logger, CancellationToken cancellationToken) =>
        await HanldeError(async () => await ExecuteQuery(iterator, cancellationToken), logger);

    [Obsolete("Use version with error handling")]
    internal static async Task<(IReadOnlyList<T> results, string? continuationToken)> ExecuteQuery<T>(this FeedIterator<T> iterator, int pageSize, CancellationToken cancellationToken)
    {
        using (iterator)
        {
            List<T> results = [];
            while (iterator.HasMoreResults)
            {
                var queryResult = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(queryResult);
                if (results.Count >= pageSize)
                {
                    return (results, queryResult.ContinuationToken);
                }
            }

            return (results, null);
        }
    }
    internal static async Task<Result<(IReadOnlyList<T> results, string? continuationToken), Error>> ExecuteQuery<T>(this FeedIterator<T> iterator, ILogger logger, int maxSize, CancellationToken cancellationToken) =>
        await HanldeError(async () => await ExecuteQuery(iterator, maxSize, cancellationToken), logger);

    internal static async Task<Result<T, Error>> HanldeError<T>(Func<Task<T>> func, ILogger logger)
    {
        try
        {
            return await func();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var errorMessage = "The resource is not found";
            logger.LogWarning(ex, "{Message}", errorMessage);
            return new Error(errorMessage, HttpStatusCode.NotFound);
        }
        catch (CosmosException ex)
        {
            var errorMessage = "Unexpected error occurred while getting data from DB";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage, ex.StatusCode);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while getting data from DB";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage, HttpStatusCode.InternalServerError);
        }
    }
}
