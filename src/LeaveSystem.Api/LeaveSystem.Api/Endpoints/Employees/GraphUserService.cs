using Ardalis.GuardClauses;
using Azure.Identity;
using Microsoft.Graph;

namespace LeaveSystem.Api.Endpoints.Employees;

public class GraphUserService
{
    private readonly string tenantId;
    private readonly string clientId;
    private readonly string secret;
    private readonly string[] scopes;

    private GraphUserService(string tenantId, string clientId, string secret, string[] scopes)
    {
        this.tenantId = tenantId;
        this.clientId = clientId;
        this.secret = secret;
        this.scopes = scopes;
    }

    public static GraphUserService Create(string? tenantId, string? clientId, string? secret, string[]? scopes)
    {
        Guard.Against.NullOrWhiteSpace(tenantId);
        Guard.Against.NullOrWhiteSpace(clientId);
        Guard.Against.NullOrWhiteSpace(secret);
        Guard.Against.NullOrEmpty(scopes);
        return new GraphUserService(tenantId, clientId, secret, scopes);
    }

    public async Task<IEnumerable<GraphUser>> Get(CancellationToken cancellationToken)
    {
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, secret, options);

        var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

        var users = await graphClient.Users.Request().GetAsync(cancellationToken);

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
