namespace LeaveSystem.Functions.GraphApi;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;

internal class GetUserRepository(IGraphClientFactory graphClientFactory) : IGetUserRepository
{
    private static readonly string[] Select = ["id", "displayName"];

    public async Task<Result<IGetUserRepository.User, Error>> GetUser(string id, CancellationToken cancellationToken)
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
}
