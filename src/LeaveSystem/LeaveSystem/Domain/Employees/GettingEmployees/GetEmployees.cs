namespace LeaveSystem.Domain.Employees.GettingEmployees;
using System.Threading;
using System.Threading.Tasks;
using GoldenEye.Backend.Core.DDD.Queries;
using LeaveSystem.Domain.Users;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;

public record GetEmployees(FederatedUser CalledBy) : IQuery<IEnumerable<FederatedUser>>;

internal class HandleGetEmployees :
    IQueryHandler<GetEmployees, IEnumerable<FederatedUser>>
{
    private readonly GetGraphUserService graphUserService;

    public HandleGetEmployees(GetGraphUserService graphUserService) => this.graphUserService = graphUserService;
    public async Task<IEnumerable<FederatedUser>> Handle(GetEmployees request, CancellationToken cancellationToken)
    {
        if (!request.CalledBy.Roles.Any())
        {
            throw new UnauthorizedAccessException("You are not permitted to access this resource");
        }
        var isPureEmployee = !IsInAllRolesExeptEmployee(request.CalledBy);
        var graphUsers = isPureEmployee ?
            await graphUserService.Get(new string[] { request.CalledBy.Id }, cancellationToken) :
            await graphUserService.Get(cancellationToken);
        return graphUsers.Where(graphUser => graphUser.Roles.Any(r => r == RoleType.Employee.ToString()));
    }
    private static bool IsInAllRolesExeptEmployee(FederatedUser federatedUser) =>
        federatedUser.Roles.IsInRoles(RoleRequirement.AllRoles.Where(r => r != RoleType.Employee).ToArray());
}
