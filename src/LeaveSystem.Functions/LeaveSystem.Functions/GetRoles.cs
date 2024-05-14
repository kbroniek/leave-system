using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            if (!ValidateToken(req.Headers.Authorization))
            {
                return new ObjectResult(new
                {
                    version = "1.0",
                    status = 401,
                    code = "errorCode",
                    requestId = "requestId",
                    userMessage = "Niepoprawne dane autoryzacyjne. Skonsultuj się z administratorem",
                    developerMessage = $"The provided code auth {req.Headers.Authorization} does not match the expected login and password."
                })
                { StatusCode = 401 };
            }
            Person person = await JsonSerializer.DeserializeAsync<Person>(req.Body, options);
            return new OkObjectResult(new { roles = new string[] { "GlobalAdmin", "TestRole", person.Email, person.Id } });
        }
        private bool ValidateToken(string header)
        {
            //Checking the header
            if (!string.IsNullOrEmpty(header) && header.StartsWith("Basic"))
            {
                //Extracting credentials
                // Removing "Basic " Substring
                string encodedUsernamePassword = header.Substring("Basic ".Length).Trim();
                //Decoding Base64
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
                //Splitting Username:Password
                int seperatorIndex = usernamePassword.IndexOf(':');
                // Extracting the individual username and password
                var username = usernamePassword.Substring(0, seperatorIndex);
                var password = usernamePassword.Substring(seperatorIndex + 1);
                //Validating the credentials
                return username == Environment.GetEnvironmentVariable("RestApiUsername") &&
                    password == Environment.GetEnvironmentVariable("RestApiPassword");
            }
            else
            {
                _logger.LogWarning("Header is missing");
                return false;
            }
        }
    }

    public record Person(string Email, string Id);
}
