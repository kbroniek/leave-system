namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System;
using System.Linq;
using System.Threading.Tasks;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;

public class EmployeeService(IRolesRepository getRolesRepository, IGetUserRepository userRepository)
{
    public async Task<Result<Employee, Error>> Get(string id, CancellationToken cancellationToken)
    {
        var getRolesTask = getRolesRepository.GetUserRoles(id, cancellationToken);
        var getUserTask = userRepository.GetUser(id, cancellationToken);

        var rolesResult = await getRolesTask;
        if (rolesResult.IsFailure)
        {
            return rolesResult.Error;
        }
        if (!rolesResult.Value.Roles.Contains(nameof(RoleType.Employee)))
        {
            return new Error($"Cannot find the employee. Id={id}", System.Net.HttpStatusCode.NotFound);
        }
        var userResult = await getUserTask;
        if (userResult.IsFailure)
        {
            return rolesResult.Error;
        }
        return new Employee(userResult.Value.Id, userResult.Value.Name);
    }
    public record Employee(string Id, string? Name);
}

public interface IGetUserRepository
{
    Task<Result<User, Error>> GetUser(string id, CancellationToken cancellationToken);
    public record User(string Id, string? Name);
}

public interface IRolesRepository
{
    Task<Result<UserRoles, Error>> GetUserRoles(string id, CancellationToken cancellationToken);
    public record UserRoles(string[] Roles);
}
