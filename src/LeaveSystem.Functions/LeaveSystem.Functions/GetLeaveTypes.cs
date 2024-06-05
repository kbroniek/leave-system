using System.Globalization;
using System.Security.Claims;
using LeaveSystem.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions
{
    public class GetLeaveTypes
    {
        private readonly ILogger _logger;

        public GetLeaveTypes(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetLeaveTypes>();
        }

        [Function("GetLeaveTypes")]
        [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},Test")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
