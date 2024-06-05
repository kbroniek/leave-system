namespace LeaveSystem.Functions.Shared;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

// TODO: Remove when it is done https://github.com/Azure/azure-functions-dotnet-worker/issues/2476
public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.logger = logger;
    }
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex)
        {
            logger.LogError(ex, "Global exception handler");
            if (!await HandleHttpError(context, ex.StatusCode, ex))
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Global exception handler");
            if (!await HandleHttpError(context, StatusCodes.Status500InternalServerError, ex))
            {
                throw;
            }
        }
    }

    private static async Task<bool> HandleHttpError(FunctionContext context, int status, Exception ex)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;
        var problemDetails = new ProblemDetails
        {
            Detail = ex.Message,
            Status = status,
            Title = "Global exception handler"
        };
        httpContext.Response.Headers.ContentType = "application/problem+json; charset=utf-8";

        await HttpResponseWritingExtensions.WriteAsync(httpContext.Response, JsonSerializer.Serialize(problemDetails, SerializeOptions));

        return true;
    }
}
