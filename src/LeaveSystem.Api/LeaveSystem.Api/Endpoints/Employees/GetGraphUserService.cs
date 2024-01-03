﻿using LeaveSystem.Api.GraphApi;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Date;
using Microsoft.Graph;
using GraphClientFactory = LeaveSystem.Api.GraphApi.GraphClientFactory;

namespace LeaveSystem.Api.Endpoints.Employees;

public class GetGraphUserService
{
    private readonly IGraphClientFactory graphClientFactory;
    private readonly string roleAttributeName;

    public GetGraphUserService(IGraphClientFactory graphClientFactory, RoleAttributeNameResolver roleAttributeNameResolver, DateService CurrentDateService)
    {
        this.graphClientFactory = graphClientFactory;
        roleAttributeName = roleAttributeNameResolver.RoleAttributeName;
    }

    public virtual async Task<IEnumerable<FederatedUser>> Get(CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        // Get all users
        var users = await graphClient.Users
            .Request()
            .Select($"id,mail,displayName,identities,{roleAttributeName}")
            .GetAsync(cancellationToken);
        var graphUsers = new List<FederatedUser>();
        var pageIterator = PageIterator<User>
            .CreatePageIterator(graphClient, users,
                (user) =>
                {
                    graphUsers.Add(new FederatedUser(user.Id, user.Mail, user.DisplayName,
                        RoleAttributeNameResolver.MapRoles(user.AdditionalData, roleAttributeName).Roles));
                    return true;
                }
            );

        await pageIterator.IterateAsync(cancellationToken);

        return graphUsers;
    }
    public virtual async Task<FederatedUser> Get(string id, CancellationToken cancellationToken)
    {
        var graphClient = graphClientFactory.Create();

        var user = await graphClient.Users[id]
            .Request()
            .Select($"id,mail,displayName,identities,{roleAttributeName}")
            .GetAsync(cancellationToken);

        return new FederatedUser(user.Id, user.Mail, user.DisplayName,
                        RoleAttributeNameResolver.MapRoles(user.AdditionalData, roleAttributeName).Roles);
    }
}
