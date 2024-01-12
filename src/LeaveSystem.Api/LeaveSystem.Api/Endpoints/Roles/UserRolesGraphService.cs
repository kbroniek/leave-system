using System.Text.Json;
using GoldenEye.Backend.Core.Exceptions;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Shared;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace LeaveSystem.Api.Endpoints.Roles;

public class UserRolesGraphService
{
    private readonly IGraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    public UserRolesGraphService(IGraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver)
    {
        this.graphClientFactory = graphClientFactory;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public async Task<IEnumerable<GraphUserRole>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var users = await graphClient.Users
            .GetAsync(_ => _.QueryParameters.Select = new string[] { "id", roleAttributeName }, cancellationToken)
            ?? throw NotFoundException.For<UserCollectionResponse>("Can't find users in graph api");
        var graphUsers = new List<GraphUserRole>();
        var pageIterator = PageIterator<User, UserCollectionResponse>
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
            { roleAttributeName, JsonSerializer.Serialize(new RolesResult(roles)) }
        };
        await graphClient.Users[userId]
            .PatchAsync(new User
            {
                AdditionalData = extensionInstance
            }, default, cancellationToken);
    }

    public record GraphUserRole(string Id, IEnumerable<string> Roles)
    {
        public static GraphUserRole Create(string id, IDictionary<string, object?>? additionalData, string roleAttributeName) =>
            new(id, RoleAttributeNameResolver.MapRoles(additionalData, roleAttributeName).Roles);
    }
}
