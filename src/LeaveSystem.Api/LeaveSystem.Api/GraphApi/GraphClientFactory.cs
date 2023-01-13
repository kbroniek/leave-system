using Ardalis.GuardClauses;
using Azure.Identity;
using Microsoft.Graph;

namespace LeaveSystem.Api.Factories;

public class GraphClientFactory
{
    private readonly string tenantId;
    private readonly string clientId;
    private readonly string secret;
    private readonly string[] scopes;

    private GraphClientFactory(string tenantId, string clientId, string secret, string[] scopes)
    {
        this.tenantId = tenantId;
        this.clientId = clientId;
        this.secret = secret;
        this.scopes = scopes;
    }

    public static GraphClientFactory Create(string? tenantId, string? clientId, string? secret, string[]? scopes)
    {
        Guard.Against.NullOrWhiteSpace(tenantId);
        Guard.Against.NullOrWhiteSpace(clientId);
        Guard.Against.NullOrWhiteSpace(secret);
        Guard.Against.NullOrEmpty(scopes);
        return new (tenantId, clientId, secret, scopes);
    }

    public GraphServiceClient Create()
    {
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            tenantId, clientId, secret, options);

        return new GraphServiceClient(clientSecretCredential, scopes);
    }
}
