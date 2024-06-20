namespace LeaveSystem.Functions.Employees;

using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class EmployeesFunction
{
    private readonly ILogger<EmployeesFunction> logger;

    public EmployeesFunction(ILogger<EmployeesFunction> logger)
    {
        this.logger = logger;
    }

    [Function("GetEmployee")]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.HumanResource)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public IActionResult GetEmployees([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "employees")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var userId = req.HttpContext.GetUserId();
        var employees = new[] {
            new GetEmployeeDto(
                "55d4c226-206d-4449-bf5d-0c0065b80ff4",
                "Jan Kowalski",
                "jan@test.pl"),
            new GetEmployeeDto(
                userId,
                req.HttpContext.User.Identity?.Name,
                "current@test.pl"),
        };

        return new OkObjectResult(employees.ToPagedListResponse());
    }
}
