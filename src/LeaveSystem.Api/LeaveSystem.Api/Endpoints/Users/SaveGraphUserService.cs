using LeaveSystem.Api.Factories;
using LeaveSystem.Shared;
using Microsoft.Graph;
using System.Text.Json;

namespace LeaveSystem.Api.Endpoints.Users;

public class SaveGraphUserService
{
    private readonly Factories.GraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    public SaveGraphUserService(Factories.GraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver)
    {
        this.graphClientFactory = graphClientFactory;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public async Task<FederatedUser> Add(string? email, string? name, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        IDictionary<string, object> extensionInstance = new Dictionary<string, object>
        {
            { roleAttributeName, JsonSerializer.Serialize(new RolesAttribute(roles)) }
        };
        var addedUser = await graphClient.Users.Request().AddAsync(new User
        {
            AdditionalData = extensionInstance,
            Mail = email,
            DisplayName = name,
            AccountEnabled = true,
            UserPrincipalName = email,
        }, cancellationToken);
        return new FederatedUser(addedUser.Id, addedUser.Mail, addedUser.DisplayName,
                        RoleAttributeNameResolver.MapRoles(addedUser.AdditionalData, roleAttributeName).Roles);
    }

    public async Task<FederatedUser> Update(string userId, string? email, string? name, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        IDictionary<string, object> extensionInstance = new Dictionary<string, object>
        {
            { roleAttributeName, JsonSerializer.Serialize(new RolesAttribute(roles)) }
        };
        var updatedUser = await graphClient.Users[userId]
            .Request()
            .UpdateAsync(new User
            {
                AdditionalData = extensionInstance,
                Mail = email,
                DisplayName = name
            }, cancellationToken);
        return new FederatedUser(updatedUser.Id, updatedUser.Mail, updatedUser.DisplayName,
                        RoleAttributeNameResolver.MapRoles(updatedUser.AdditionalData, roleAttributeName).Roles);
    }
}
