using System.Text;
using DarkLoop.Azure.Functions.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions
{
    [FunctionAuthorize]
    public class TestFunction
    {
        private readonly ILogger<TestFunction> _logger;

        public TestFunction(ILogger<TestFunction> logger)
        {
            _logger = logger;
        }

        [Function("TestFunction")]
        [Authorize]
        public async Task<IActionResult> Run([HttpTrigger("get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var provider = req.HttpContext.RequestServices;
            var schProvider = provider.GetService<IAuthenticationSchemeProvider>();

            var sb = new StringBuilder();
            sb.AppendLine("Authentication schemes:");

            if (schProvider is not null)
            {
                foreach (var scheme in await schProvider.GetAllSchemesAsync())
                    sb.AppendLine($"  {scheme.Name} -> {scheme.HandlerType}");
            }

            sb.AppendLine();
            sb.AppendLine($"User:");
            sb.AppendLine($"  Name  -> {req.HttpContext.User.Identity!.Name}");
            sb.AppendLine($"  Email -> {req.HttpContext.User.FindFirst("emails")?.Value}");
            sb.AppendLine($"  Name  -> {req.HttpContext.User.IsInRole("{\"Roles\":[\"GlobalAdmin\"]}")}");


            foreach (var claim in req.HttpContext.User.Claims)
                sb.AppendLine($"  {claim.Type} -> {claim.Value}");

            return new OkObjectResult(sb.ToString());
        }
    }
}