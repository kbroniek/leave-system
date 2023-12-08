using Ardalis.GuardClauses;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Shared;
using Microsoft.Graph;
using System.Text.Json;

namespace LeaveSystem.Api.Endpoints.Users;

public class SaveGraphUserService
{
    private readonly IGraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;
    private readonly string defaultPassword;
    private readonly string issuer;

    private SaveGraphUserService(IGraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver, string defaultPassword, string issuer)
    {
        this.graphClientFactory = graphClientFactory;
        this.defaultPassword = defaultPassword;
        this.issuer = issuer;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public static SaveGraphUserService Create(IGraphClientFactory? graphClientFactory,
        RoleAttributeNameResolver? roleAttributeNameResolver, string? defaultPassword, string? issuer)
        => new(Guard.Against.Nill(graphClientFactory),
            Guard.Against.Nill(roleAttributeNameResolver),
            Guard.Against.NullOrWhiteSpace(defaultPassword),
            Guard.Against.NullOrWhiteSpace(issuer));

    public async Task<FederatedUser> Add(string email, string? name, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        IDictionary<string, object> extensionInstance = new Dictionary<string, object>
        {
            { roleAttributeName, JsonSerializer.Serialize(new RolesAttribute(roles)) }
        };
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
                Password = defaultPassword
            },
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
            PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword",
            UserPrincipalName = $"{principalId}@{issuer}",
        }, cancellationToken);
        return new FederatedUser(addedUser.Id, addedUser.Mail, addedUser.DisplayName,
            RoleAttributeNameResolver.MapRoles(addedUser.AdditionalData, roleAttributeName).Roles);
    }

    public async Task<FederatedUser> Update(string userId, string email, string? name, IEnumerable<string> roles, CancellationToken cancellationToken)
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
