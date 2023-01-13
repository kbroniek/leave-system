using LeaveSystem.Api.Factories;
using LeaveSystem.Shared;
using Microsoft.Graph;

namespace LeaveSystem.Api.Endpoints.Employees;

public class GetGraphUserService
{
    private readonly Factories.GraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    public GetGraphUserService(Factories.GraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver)
    {
        this.graphClientFactory = graphClientFactory;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public async Task<IEnumerable<FederatedUser>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        // Get all users
        var users = await graphClient.Users
            .Request()
            .Select(e => new
            {
                e.Id,
                e.Mail,
                e.DisplayName,
                e.Identities, //TODO: Get emails from here
                e.AdditionalData
            })
            .GetAsync(cancellationToken);
        var graphUsers = new List<FederatedUser>();
        var pageIterator = PageIterator<User>
            .CreatePageIterator(graphClient, users,
                (user) =>
                {
                    graphUsers.Add(new FederatedUser(user.Id, user.Mail, user.DisplayName,
                        RoleAttributeNameResolver.MapRoles(users.AdditionalData, roleAttributeName).Roles));
                    return true;
                }
            );

        await pageIterator.IterateAsync(cancellationToken);

        return graphUsers;
    }
}
