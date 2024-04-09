namespace LeaveSystem.Functions;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class Function1
{
    private readonly ILogger<Function1> logger;

    public Function1(ILogger<Function1> logger)
    {
        this.logger = logger;
    }

    [Function("Function1")]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("Welcome to .NET isolated worker !!");

        return response;
    }
}
