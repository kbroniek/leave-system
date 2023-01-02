using LeaveSystem.Api.Factories;

namespace LeaveSystem.Api.Endpoints.Employees;

public class GetGraphUserService
{
    private readonly GraphClientFactory graphClientFactory;

    public GetGraphUserService(GraphClientFactory graphClientFactory)
        => this.graphClientFactory = graphClientFactory;

    public async Task<IEnumerable<GraphUser>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var users = await graphClient.Users
            .Request()
            .Select(u => new { u.Id, u.Mail, u.DisplayName })
            .GetAsync(cancellationToken);

        var graphUsers = Enumerable.Empty<GraphUser>();
        while (users is not null && users.Count != 0)
        {
            graphUsers = graphUsers.Union(users.Select(u => new GraphUser(u.Id, u.Mail, u.DisplayName)));
            if (users.NextPageRequest is null)
            {
                return graphUsers.ToList();
            }
            users = await users.NextPageRequest.GetAsync(cancellationToken);
        }
        return graphUsers;
    }
}

public record class GraphUser(string Id, string Email, string DisplayName);
