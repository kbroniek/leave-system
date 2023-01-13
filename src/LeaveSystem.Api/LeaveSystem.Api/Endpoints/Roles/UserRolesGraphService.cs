using LeaveSystem.Api.Factories;
using LeaveSystem.Shared;
using Microsoft.Graph;
using System.Text.Json;

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
            .Select(e => new { e.Id, e.AdditionalData })
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
    public async Task Update(string userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        IDictionary<string, object> extensionInstance = new Dictionary<string, object>
        {
            { roleAttributeName, JsonSerializer.Serialize(new RolesAttribute(roles)) }
        };
        await graphClient.Users[userId]
            .Request()
            .UpdateAsync(new User
            {
                AdditionalData = extensionInstance
            }, cancellationToken);
    }

    public record class GraphUserRole(string Id, IEnumerable<string> Roles)
    {
        public static GraphUserRole Create(string id, IDictionary<string, object>? additionalData, string roleAttributeName) =>
            new(id, RoleAttributeNameResolver.MapRoles(additionalData, roleAttributeName).Roles);
    }
}
