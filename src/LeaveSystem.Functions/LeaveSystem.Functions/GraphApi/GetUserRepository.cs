namespace LeaveSystem.Functions.GraphApi;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

internal class GetUserRepository(IGraphClientFactory graphClientFactory, ILogger<GetUserRepository> logger) : IGetUserRepository
{
    private static readonly string[] Select = ["id", "displayName"];

    public async Task<Result<IGetUserRepository.User, Error>> GetUser(string id, CancellationToken cancellationToken)
    {
        try
        {
            var graphClient = graphClientFactory.Create();
            var user = await graphClient.Users[id]
                .GetAsync(_ => _.QueryParameters.Select = Select, cancellationToken);
            if (user is null)
            {
                return new Error($"Cannot find the graph user. UserId={id}", System.Net.HttpStatusCode.NotFound);
            }
            return new IGetUserRepository.User(user.Id ?? id, user.DisplayName);
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            var errorMessage = "The resource is not found";
            logger.LogError(ex, "{Message}", $"{errorMessage}. UserId={id}");
            return new Error(errorMessage, System.Net.HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while getting data from GraphApi";
            logger.LogError(ex, "{Message}", $"{errorMessage}. UserId={id}");
            return new Error(errorMessage, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IReadOnlyCollection<IGetUserRepository.User>, Error>> GetUsers(string[] ids, CancellationToken cancellationToken)
    {
        try
        {
            var graphClient = graphClientFactory.Create();
            var users = await graphClient.Users
                .GetAsync(_ =>
                {
                    _.QueryParameters.Select = ["id", "displayName", "givenName", "surname"];
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
            return new Error(errorMessage, System.Net.HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while getting data from GraphApi";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage, System.Net.HttpStatusCode.InternalServerError);
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
            new(user.Id, user.DisplayName, user.GivenName, user.Surname);
}
