namespace LeaveSystem.Functions.GraphApi;

using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

internal class UpdateUserRepository(IGraphClientFactory graphClientFactory, ILogger<UpdateUserRepository> logger)
{
    public async Task<Result<Error>> UpdateUser(string id, UpdateUserDto updateUserDto, CancellationToken cancellationToken)
    {
        try
        {
            var graphClient = graphClientFactory.Create();
            var userUpdate = new User();

            if (updateUserDto.AccountEnabled.HasValue)
            {
                userUpdate.AccountEnabled = updateUserDto.AccountEnabled.Value;
            }

            if (updateUserDto.JobTitle is not null)
            {
                userUpdate.JobTitle = updateUserDto.JobTitle;
            }

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
            var errorMessage = "Insufficient permissions to update user";
            logger.LogError(ex, "{Message}. UserId={UserId}", errorMessage, id);
            return new Error(errorMessage, System.Net.HttpStatusCode.Forbidden, ErrorCodes.FORBIDDEN_OPERATION);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while updating user in GraphApi";
            logger.LogError(ex, "{Message}. UserId={UserId}", errorMessage, id);
            return new Error(errorMessage, System.Net.HttpStatusCode.InternalServerError, ErrorCodes.UNEXPECTED_GRAPH_ERROR);
        }
    }
}

