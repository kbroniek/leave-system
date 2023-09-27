using System.Net;
using System.Text.Json;
using LeaveSystem.Api.Endpoints.Errors;

namespace LeaveSystem.Api.Extensions;

internal static class HttpContextExtensions
{
    public static async Task HandleExceptionAsync(this HttpContext context, HttpStatusCode statusCode, string message = "")
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var error = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = message
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}