namespace LeaveSystem.Functions.GraphApi;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using Microsoft.Extensions.Logging;
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
                return new Error($"Cannot find the user. UserId={id}", System.Net.HttpStatusCode.NotFound);
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
}
