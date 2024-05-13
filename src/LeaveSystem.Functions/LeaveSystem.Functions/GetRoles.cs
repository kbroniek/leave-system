using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions
{
    public class GetRoles
    {
        private readonly ILogger<GetRoles> _logger;

        public GetRoles(ILogger<GetRoles> logger)
        {
            _logger = logger;
        }

        [Function("GetRoles")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult(new { roles = new string[] { "GlobalAdmin", "TestRole" } });
        }
    }
}
