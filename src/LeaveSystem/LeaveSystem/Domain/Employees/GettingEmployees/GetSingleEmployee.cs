namespace LeaveSystem.Domain.Employees.GettingEmployees;
using System.Threading;
using System.Threading.Tasks;
using GoldenEye.Backend.Core.DDD.Queries;
using LeaveSystem.Domain.Users;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;

public record GetSingleEmployee(FederatedUser CalledBy, string Id) : IQuery<FederatedUser>;

internal class HandleGetSingleEmployee :
    IQueryHandler<GetSingleEmployee, FederatedUser>
{
    private readonly GetGraphUserService graphUserService;

    public HandleGetSingleEmployee(GetGraphUserService graphUserService) => this.graphUserService = graphUserService;
    public async Task<FederatedUser> Handle(GetSingleEmployee request, CancellationToken cancellationToken)
    {
        var canGetAllUsers = IsInAllRolesExeptEmployee(request.CalledBy);
        if (!canGetAllUsers && !string.Equals(request.CalledBy.Id, request.Id, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You are not authorize to get different user.");
        }
        return await graphUserService.Get(request.Id, cancellationToken);
    }
    private static bool IsInAllRolesExeptEmployee(FederatedUser federatedUser) =>
        federatedUser.Roles.IsInRoles(RoleRequirement.AllRoles.Where(r => r != RoleType.Employee).ToArray());
}
