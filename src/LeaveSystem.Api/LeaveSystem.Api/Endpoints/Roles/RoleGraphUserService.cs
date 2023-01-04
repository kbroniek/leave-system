using Ardalis.GuardClauses;
using LeaveSystem.Api.Factories;
using LeaveSystem.Shared;
using Microsoft.Graph;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LeaveSystem.Api.Endpoints.Roles;

public class RoleGraphUserService
{
    private const string RoleAttributeName = "Role";
    private readonly Factories.GraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    private record RoleAttribute(IEnumerable<string> Roles);

    private RoleGraphUserService(Factories.GraphClientFactory graphClientFactory, string roleAttributeName)
    {
        this.graphClientFactory = graphClientFactory;
        this.roleAttributeName = roleAttributeName;
    }

    internal static RoleGraphUserService Create(Factories.GraphClientFactory? graphClientFactory, string? b2cExtensionAppClientId)
    {
        Guard.Against.Nill(graphClientFactory);
        Guard.Against.NullOrEmpty(b2cExtensionAppClientId);
        string roleAttributeName = GetCompleteAttributeName(b2cExtensionAppClientId, RoleAttributeName);
        return new(graphClientFactory, roleAttributeName);
    }

    public async Task<IEnumerable<string>> Get(string userId, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        var user = await graphClient.Users[userId]
            .Request()
            .Select($"{roleAttributeName}")
            .GetAsync(cancellationToken);
        var rolesRaw = user.AdditionalData[roleAttributeName].ToString();
        return JsonSerializer.Deserialize<RoleAttribute>(rolesRaw)?.Roles;
    }
    public async Task Update(string userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();
        IDictionary<string, object> extensionInstance = new Dictionary<string, object>();
        extensionInstance.Add(roleAttributeName, JsonSerializer.Serialize(new RoleAttribute(roles)));
        await graphClient.Users[userId]
            .Request()
            .UpdateAsync(new User
            {
                AdditionalData = extensionInstance
            }, cancellationToken);
    }

    private static string GetCompleteAttributeName(string b2cExtensionAppClientId, string attributeName)
    {
        Guard.Against.NullOrWhiteSpace(b2cExtensionAppClientId);
        Guard.Against.NullOrWhiteSpace(attributeName);
        b2cExtensionAppClientId = b2cExtensionAppClientId.Replace("-", "");
        return $"extension_{b2cExtensionAppClientId}_{attributeName}";
    }
}
