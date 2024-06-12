namespace LeaveSystem.Functions.Employees;

using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Dto;
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
    public IActionResult GetEmployee([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
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

        return new OkObjectResult(employees.CreatePagedListResponse());
    }
}
