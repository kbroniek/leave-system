using LeaveSystem.Api.Factories;
using Microsoft.Graph;

namespace LeaveSystem.Api.Endpoints.Roles;

public class UserRolesGraphService
{
    private readonly Factories.GraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    public UserRolesGraphService(Factories.GraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver)
    {
        this.graphClientFactory = graphClientFactory;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public async Task<IEnumerable<GraphUserRole>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var users = await graphClient.Users
            .Request()
            .Select($"id,{roleAttributeName}")
            .GetAsync(cancellationToken);
        var graphUsers = new List<GraphUserRole>();
        var pageIterator = PageIterator<User>
            .CreatePageIterator(graphClient, users,
                (user) =>
                {
                    graphUsers.Add(GraphUserRole.Create(user.Id, user.AdditionalData, roleAttributeName));
                    return true;
                }
            );

        await pageIterator.IterateAsync(cancellationToken);

        return graphUsers;
    }

    public record class GraphUserRole(string Id, IEnumerable<string> Roles)
    {
        public static GraphUserRole Create(string id, IDictionary<string, object>? additionalData, string roleAttributeName) =>
            new(id, RoleAttributeNameResolver.MapRoles(additionalData, roleAttributeName).Roles);
    }
}
