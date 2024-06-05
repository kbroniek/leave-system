using LeaveSystem.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions.LeaveRequests
{
    public class LeaveRequestsFunction
    {
        private readonly ILogger<LeaveRequestsFunction> _logger;

        public LeaveRequestsFunction(ILogger<LeaveRequestsFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SearchLeaveRequests))]
        [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.HumanResource)}")]
        public async Task<IActionResult> SearchLeaveRequests([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var queryResult = SearchLeaveRequestsQuery.Bind(req.HttpContext);
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
