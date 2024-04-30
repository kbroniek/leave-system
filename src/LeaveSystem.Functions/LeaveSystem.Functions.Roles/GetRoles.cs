using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions
{
    public class GetRoles
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private static readonly CosmosLinqSerializerOptions LinqSerializerOptions = new()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        };
        private readonly ILogger<GetRoles> logger;

        public GetRoles(ILogger<GetRoles> logger) => this.logger = logger;

        [Function(nameof(GetRoles))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [CosmosDBInput(
                databaseName: "LeaveSystem",
                containerName: "Roles",
                Connection = "CosmosDBConnection")] Container container)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            if (!ValidateToken(req.Headers.Authorization))
            {
                logger.LogWarning("Wrong RestApi credentials.");
                return new ObjectResult(new
                {
                    version = "1.0",
                    status = 401,
                    code = "errorCode",
                    requestId = "requestId",
                    userMessage = "Wrong RestApi credentials. Contact with administrators.",
                    developerMessage = $"The provided code auth {req.Headers.Authorization} does not match the expected login and password."
                })
                { StatusCode = 401 };
            }
            var userId = await JsonSerializer.DeserializeAsync<UserId>(req.Body, JsonSerializerOptions);
            if (userId is null)
            {
                return new NotFoundObjectResult("userId");
            }
            logger.LogInformation($"Deserialized user id {userId.Id}.");
            var iterator = container.GetItemLinqQueryable<RolesModel>(linqSerializerOptions: LinqSerializerOptions)
                .Where(r => r.Id == userId.Id)
                .ToFeedIterator();
            var roles = iterator.HasMoreResults ?
                (await iterator.ReadNextAsync()).FirstOrDefault()?.Roles :
                null;
            return new OkObjectResult(new { roles = roles ?? Enumerable.Empty<string>() });
        }
        private bool ValidateToken(string? header)
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
                var username = usernamePassword[..seperatorIndex];
                var password = usernamePassword[(seperatorIndex + 1)..];
                //Validating the credentials
                return username == Environment.GetEnvironmentVariable("RestApiUsername") &&
                    password == Environment.GetEnvironmentVariable("RestApiPassword");
            }
            else
            {
                logger.LogWarning("Header is missing");
                return false;
            }
        }
    }

    public record UserId(Guid Id);
    public record RolesModel(Guid Id, string[] Roles);
}
