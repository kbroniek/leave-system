using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using Microsoft.Graph;
using System.Text.Json;

namespace LeaveSystem.Api.Endpoints.Roles;

public class UserRolesGraphService
{
    private const string RoleAttributeName = "Role";
    private readonly Factories.GraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    private UserRolesGraphService(Factories.GraphClientFactory graphClientFactory, string roleAttributeName)
    {
        this.graphClientFactory = graphClientFactory;
        this.roleAttributeName = roleAttributeName;
    }

    public static UserRolesGraphService Create(Factories.GraphClientFactory? graphClientFactory, string? b2cExtensionAppClientId)
    {
        Guard.Against.Nill(graphClientFactory);
        Guard.Against.NullOrEmpty(b2cExtensionAppClientId);
        string roleAttributeName = GetCompleteAttributeName(b2cExtensionAppClientId, RoleAttributeName);
        return new(graphClientFactory, roleAttributeName);
    }


    public async Task<IEnumerable<GraphUserRole>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var users = await graphClient.Users
            .Request()
            .Select(e => new { e.Id, e.AdditionalData })
            .GetAsync(cancellationToken);
        var graphUsers = new List<GraphUserRole>();
        // Iterate over all the users in the directory
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

    private static string GetCompleteAttributeName(string? b2cExtensionAppClientId, string? attributeName)
    {
        Guard.Against.NullOrWhiteSpace(b2cExtensionAppClientId);
        Guard.Against.NullOrWhiteSpace(attributeName);
        b2cExtensionAppClientId = b2cExtensionAppClientId.Replace("-", "");
        return $"extension_{b2cExtensionAppClientId}_{attributeName}";
    }

    public record class GraphUserRole(string Id, IEnumerable<string> Roles)
    {
        public static GraphUserRole Create(string id, IDictionary<string, object> additionalData, string roleAttributeName)
        {
            if (additionalData.TryGetValue(roleAttributeName, out object? rolesRaw) && rolesRaw is not null)
            {
                return new(id, ClaimsPrincipalExtensions.Create(rolesRaw.ToString()));
            }
            return new(id, RolesAttribute.Empty.Roles);
        }
    }
}
