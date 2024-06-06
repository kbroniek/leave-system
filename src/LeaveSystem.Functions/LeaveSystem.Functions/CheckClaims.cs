using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions
{
    public class CheckClaims
    {
        private readonly ILogger<CheckClaims> _logger;

        public CheckClaims(ILogger<CheckClaims> logger)
        {
            _logger = logger;
        }

        [Function("CheckClaims")]
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
            sb.AppendLine($"  Id  -> {req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value}");


            foreach (var claim in req.HttpContext.User.Claims)
                sb.AppendLine($"  {claim.Type} -> {claim.Value}");

            return new OkObjectResult(sb.ToString());
        }
    }
}
