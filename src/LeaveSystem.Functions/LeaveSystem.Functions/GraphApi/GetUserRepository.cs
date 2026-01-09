namespace LeaveSystem.Functions.GraphApi;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

internal class GetUserRepository(IGraphClientFactory graphClientFactory, ILogger<GetUserRepository> logger, IMemoryCache memoryCache) : IGetUserRepository
{
    private static readonly TimeSpan AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    private static readonly string[] Select = ["id", "displayName", "accountEnabled", "mail", "userPrincipalName"];

    public async Task<Result<IGetUserRepository.User, Error>> GetUser(string id, CancellationToken cancellationToken)
    {
        try
        {
            var graphClient = graphClientFactory.Create();
            var user = await graphClient.Users[id]
                .GetAsync(_ => _.QueryParameters.Select = Select, cancellationToken);
            if (user is null)
            {
                return new Error($"Cannot find the graph user. UserId={id}", System.Net.HttpStatusCode.NotFound, ErrorCodes.GRAPH_USER_NOT_FOUND);
            }
            return new IGetUserRepository.User(user.Id ?? id, user.DisplayName, null, null, null, user.AccountEnabled, user.Mail ?? user.UserPrincipalName);
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            var errorMessage = "The resource is not found";
            logger.LogError(ex, "{Message}", $"{errorMessage}. UserId={id}");
            return new Error(errorMessage, System.Net.HttpStatusCode.NotFound, ErrorCodes.GRAPH_USER_NOT_FOUND);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while getting data from GraphApi";
            logger.LogError(ex, "{Message}", $"{errorMessage}. UserId={id}");
            return new Error(errorMessage, System.Net.HttpStatusCode.InternalServerError, ErrorCodes.UNEXPECTED_GRAPH_ERROR);
        }
    }

    public async Task<Result<IReadOnlyCollection<IGetUserRepository.User>, Error>> GetUsers(string[] ids, CancellationToken cancellationToken)
    {
        var allUsers = await GetUsersFromGraphCache(graphClientFactory, logger, [], cancellationToken);
        if (ids.Length == 0)
        {
            return allUsers;
        }

        return allUsers.Success(users =>
            (IReadOnlyCollection<IGetUserRepository.User>)[.. users.Where(u => ids.Contains(u.Id))]);
    }

    private async ValueTask<Result<IReadOnlyCollection<IGetUserRepository.User>, Error>> GetUsersFromGraphCache(IGraphClientFactory graphClientFactory, ILogger<GetUserRepository> logger, string[] ids, CancellationToken cancellationToken)
    {
        var cacheKey = "users_all";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<IGetUserRepository.User>? cachedResult))
        {
            return Result.Ok<IReadOnlyCollection<IGetUserRepository.User>, Error>(cachedResult!);
        }

        var result = await GetUsersFromGraph(graphClientFactory, logger, ids, cancellationToken);
        if (result.IsSuccess)
        {
            memoryCache.Set(cacheKey, result.Value, AbsoluteExpirationRelativeToNow);
        }
        return result;

    }

    private static async Task<Result<IReadOnlyCollection<IGetUserRepository.User>, Error>> GetUsersFromGraph(IGraphClientFactory graphClientFactory, ILogger<GetUserRepository> logger, string[] ids, CancellationToken cancellationToken)
    {
        try
        {
            var graphClient = graphClientFactory.Create();
            var users = await graphClient.Users
                .GetAsync(_ =>
                {
                    _.QueryParameters.Select = ["id", "displayName", "givenName", "surname", "jobTitle", "accountEnabled", "mail", "userPrincipalName"];
                    //TODO: Only 15 items are allowed in the filter.
                    _.QueryParameters.Filter = ids.Length == 0 ? null : $"id in ({string.Join(",", ids.Select(id => $"'{id}'"))})";
                }, cancellationToken);
            if (users is null)
            {
                return Array.Empty<IGetUserRepository.User>();
            }
            var result = await GetAll(graphClient, users, cancellationToken);
            return result;
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            var errorMessage = "The resource is not found";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage, System.Net.HttpStatusCode.NotFound, ErrorCodes.GRAPH_USER_NOT_FOUND);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while getting data from GraphApi";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage, System.Net.HttpStatusCode.InternalServerError, ErrorCodes.UNEXPECTED_GRAPH_ERROR);
        }
    }

    private static async Task<List<IGetUserRepository.User>> GetAll(GraphServiceClient graphClient, UserCollectionResponse users, CancellationToken cancellationToken)
    {
        var graphUsers = new List<IGetUserRepository.User>();
        var pageIterator = PageIterator<User, UserCollectionResponse>
            .CreatePageIterator(graphClient, users,
                (user) =>
                {
                    graphUsers.Add(CreateUserModel(user));
                    return true;
                }
            );

        await pageIterator.IterateAsync(cancellationToken);
        return graphUsers;
    }
    private static IGetUserRepository.User CreateUserModel(User user) =>
            new(user.Id, user.DisplayName, user.GivenName, user.Surname, user.JobTitle, user.AccountEnabled, user.Mail ?? user.UserPrincipalName);
}
