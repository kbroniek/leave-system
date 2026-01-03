namespace LeaveSystem.Functions.GraphApi;

using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

internal class UpdateUserRepository(IGraphClientFactory graphClientFactory, ILogger<UpdateUserRepository> logger)
{
    public async Task<Result<Error>> EnableUser(string id, bool accountEnabled, CancellationToken cancellationToken)
    {
        try
        {
            var graphClient = graphClientFactory.Create();
            var userUpdate = new User
            {
                AccountEnabled = accountEnabled
            };

            await graphClient.Users[id]
                .PatchAsync(userUpdate, cancellationToken: cancellationToken);

            return Result.Default;
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            var errorMessage = "The user was not found";
            logger.LogError(ex, "{Message}. UserId={UserId}", errorMessage, id);
            return new Error(errorMessage, System.Net.HttpStatusCode.NotFound, ErrorCodes.GRAPH_USER_NOT_FOUND);
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == 403)
        {
            var errorMessage = "Insufficient permissions to disable user";
            logger.LogError(ex, "{Message}. UserId={UserId}", errorMessage, id);
            return new Error(errorMessage, System.Net.HttpStatusCode.Forbidden, ErrorCodes.FORBIDDEN_OPERATION);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while disabling user in GraphApi";
            logger.LogError(ex, "{Message}. UserId={UserId}", errorMessage, id);
            return new Error(errorMessage, System.Net.HttpStatusCode.InternalServerError, ErrorCodes.UNEXPECTED_GRAPH_ERROR);
        }
    }
}

