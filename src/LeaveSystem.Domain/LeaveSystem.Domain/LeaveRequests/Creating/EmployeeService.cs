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
            return new Error($"Cannot find the employee. Id={id}", System.Net.HttpStatusCode.NotFound, ErrorCodes.EMPLOYEE_NOT_FOUND);
        }
        var userResult = await getUserTask;
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }
        if (userResult.Value.AccountEnabled != true)
        {
            return new Error($"Employee account is disabled. Id={id}", System.Net.HttpStatusCode.Forbidden, ErrorCodes.EMPLOYEE_ACCOUNT_DISABLED);

        }
        return new Employee(userResult.Value.Id, userResult.Value.Name);
    }
    public record Employee(string Id, string? Name);
}

public interface IGetUserRepository
{
    Task<Result<User, Error>> GetUser(string id, CancellationToken cancellationToken);
    Task<Result<IReadOnlyCollection<User>, Error>> GetUsers(string[] ids, CancellationToken cancellationToken);
    public record User(string Id, string? Name, string? FirstName, string? LastName, string? JobTitle, bool? AccountEnabled, string? Email);
}

public interface IRolesRepository
{
    Task<Result<UserRoles, Error>> GetUserRoles(string id, CancellationToken cancellationToken);
    public record UserRoles(string[] Roles);
}
