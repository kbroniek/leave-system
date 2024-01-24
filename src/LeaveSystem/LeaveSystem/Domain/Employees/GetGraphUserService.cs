using GoldenEye.Backend.Core.Exceptions;
using LeaveSystem.GraphApi;
using LeaveSystem.Shared;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace LeaveSystem.Domain.Employees;

public class GetGraphUserService
{
    private readonly IGraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    public GetGraphUserService(IGraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver)
    {
        this.graphClientFactory = graphClientFactory;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public virtual async Task<IEnumerable<FederatedUser>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var users = await graphClient.Users
            .GetAsync(_ => _.QueryParameters.Select = new string[] { "id", "mail", "displayName", "identities", roleAttributeName }, cancellationToken)
            ?? throw NotFoundException.For<UserCollectionResponse>("Can't find users in graph api");
        return await GetAll(graphClient, users, cancellationToken);
    }
    public virtual async Task<IEnumerable<FederatedUser>> Get(string[] ids, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var users = await graphClient.Users
            .GetAsync(_ =>
            {
                _.QueryParameters.Select = new string[] { "id", "mail", "displayName", "identities", roleAttributeName };
                _.QueryParameters.Filter = $"id in ({string.Join(",", ids.Select(id => $"'{id}'"))})";
            }, cancellationToken)
            ?? throw NotFoundException.For<UserCollectionResponse>("Can't find users in graph api");
        return await GetAll(graphClient, users, cancellationToken);
    }

    public virtual async Task<FederatedUser> Get(string id, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var user = await graphClient.Users[id]
            .GetAsync(_ => _.QueryParameters.Select = new string[] { "id", "mail", "displayName", "identities", roleAttributeName }, cancellationToken)
            ?? throw NotFoundException.For<UserCollectionResponse>(id);
        return CreateFederatedUser(user);
    }

    private FederatedUser CreateFederatedUser(User user) =>
            new(user.Id, user.Mail, user.DisplayName,
                            RoleAttributeNameResolver.MapRoles(user.AdditionalData, roleAttributeName).Roles);
    private async Task<List<FederatedUser>> GetAll(GraphServiceClient graphClient, UserCollectionResponse users, CancellationToken cancellationToken)
    {
        var graphUsers = new List<FederatedUser>();
        var pageIterator = PageIterator<User, UserCollectionResponse>
            .CreatePageIterator(graphClient, users,
                (user) =>
                {
                    graphUsers.Add(CreateFederatedUser(user));
                    return true;
                }
            );

        await pageIterator.IterateAsync(cancellationToken);
        return graphUsers;
    }
}
