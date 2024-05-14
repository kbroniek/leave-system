using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions
{
    public class GetRoles
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        private readonly ILogger<GetRoles> _logger;

        public GetRoles(ILogger<GetRoles> logger)
        {
            _logger = logger;
        }

        [Function("GetRoles")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            Person person = await JsonSerializer.DeserializeAsync<Person>(req.Body, options);
            return new OkObjectResult(new { roles = new string[] { "GlobalAdmin", "TestRole", person.Email, person.Id } });
        }
    }

    public record Person(string Email, string Id);
}
