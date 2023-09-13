using LeaveSystem.Shared;
using Microsoft.Graph;
using System.Text.Json;
using LeaveSystem.Api.GraphApi;

namespace LeaveSystem.Api.Endpoints.Users;

public class SaveGraphUserService
{
    private readonly IGraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    public SaveGraphUserService(IGraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver)
    {
        this.graphClientFactory = graphClientFactory;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public async Task<FederatedUser> Add(string email, string? name, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        IDictionary<string, object> extensionInstance = new Dictionary<string, object>
        {
            { roleAttributeName, JsonSerializer.Serialize(new RolesAttribute(roles)) }
        };
        try
        {
            var principalId = Guid.NewGuid();
            var addedUser = await graphClient.Users.Request().AddAsync(new User
            {
                AdditionalData = extensionInstance,
                Mail = email,
                DisplayName = name,
                AccountEnabled = true,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = "$illy-Wood?" // TODO: Get from config
                },
                MailNickname = email.Split('@').First(),
                Identities = new ObjectIdentity[]
                {
                    new ObjectIdentity
                    {
                        Issuer = GraphApi.Config.Issuer,
                        IssuerAssignedId = email,
                        SignInType = "emailAddress"
                    }
                },
                PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword",
                UserPrincipalName = $"{principalId}@{GraphApi.Config.Issuer}",
            }, cancellationToken);
            return new FederatedUser(addedUser.Id, addedUser.Mail, addedUser.DisplayName,
                            RoleAttributeNameResolver.MapRoles(addedUser.AdditionalData, roleAttributeName).Roles);
        }
        catch (ServiceException ex)
        {
            // TODO: handle HTML status codes
            throw;

        }
    }

    public async Task<FederatedUser> Update(string userId, string email, string? name, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        IDictionary<string, object> extensionInstance = new Dictionary<string, object>
        {
            { roleAttributeName, JsonSerializer.Serialize(new RolesAttribute(roles)) }
        };
        var issuer = "leavesystem.onmicrosoft.com";
        await graphClient.Users[userId]
            .Request()
            .UpdateAsync(new User
            {
                AdditionalData = extensionInstance,
                Mail = email,
                DisplayName = name,
                MailNickname = email.Split('@').First(),
                Identities = new ObjectIdentity[]
                {
                    new ObjectIdentity
                    {
                        Issuer = issuer,
                        IssuerAssignedId = email,
                        SignInType = "emailAddress"
                    }
                },
            }, cancellationToken);
        return new FederatedUser(userId, email, name, roles);
    }
}
