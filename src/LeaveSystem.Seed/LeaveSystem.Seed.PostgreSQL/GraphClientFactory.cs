namespace LeaveSystem.Seed.PostgreSQL;

using Azure.Identity;
using Microsoft.Graph;

public class GraphClientFactory(string tenantId, string clientId, string secret, string[] scopes)
{
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
