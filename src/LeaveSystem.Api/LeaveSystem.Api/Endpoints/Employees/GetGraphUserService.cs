using Microsoft.Graph;

namespace LeaveSystem.Api.Endpoints.Employees;

public class GetGraphUserService
{
    private readonly Factories.GraphClientFactory graphClientFactory;

    public GetGraphUserService(Factories.GraphClientFactory graphClientFactory)
        => this.graphClientFactory = graphClientFactory;

    public async Task<IEnumerable<GraphUser>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        // Get all users
        var users = await graphClient.Users
            .Request()
            .Select(e => new
            {
                e.Id,
                e.Mail,
                e.DisplayName,
                e.Identities //TODO: Get emails from here
            })
            .GetAsync(cancellationToken);
        var graphUsers = new List<GraphUser>();
        // Iterate over all the users in the directory
        var pageIterator = PageIterator<User>
            .CreatePageIterator(graphClient, users,
                (user) =>
                {
                    graphUsers.Add(new GraphUser(user.Id, user.Mail, user.DisplayName));
                    return true;
                }
            );

        await pageIterator.IterateAsync(cancellationToken);

        return graphUsers;
    }
}

public record class GraphUser(string Id, string Email, string DisplayName);
