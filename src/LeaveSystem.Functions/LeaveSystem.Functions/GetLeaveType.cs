namespace LeaveSystem.Functions;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class GetLeaveType
{
    private readonly ILogger<GetLeaveType> logger;

    public GetLeaveType(ILogger<GetLeaveType> logger)
    {
        this.logger = logger;
    }

    [Function("GetLeaveType")]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("Welcome to .NET isolated worker !!");

        return response;
    }
}
