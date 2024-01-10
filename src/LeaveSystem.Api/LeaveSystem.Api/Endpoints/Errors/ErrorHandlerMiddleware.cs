using System.Reflection;

namespace LeaveSystem.Api.Endpoints.Errors;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate next;
    private readonly bool isDevelopment;
    private readonly ILogger logger;

    public ErrorHandlerMiddleware(RequestDelegate next, bool isDevelopment, ILogger logger)
    {
        this.next = next;
        this.isDevelopment = isDevelopment;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (ArgumentOutOfRangeException e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status416RequestedRangeNotSatisfiable, e);
        }
        catch (GoldenEye.Backend.Core.Exceptions.NotFoundException e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status404NotFound, e, "Resource not found");
        }
        catch (ArgumentException e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status400BadRequest, e);
        }
        catch (BadHttpRequestException e)
        {
            await HandleExceptionAsync(httpContext, e.StatusCode, e);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status500InternalServerError, e,
                "Internal Server Error from the custom middleware.");
        }
    }
    public async Task HandleExceptionAsync(HttpContext context, int statusCode, Exception error, string? message = null)
    {
        logger.LogError(error, "{Message}", message);
        await Results.Problem(isDevelopment ? error.ToString() : null, nameof(ErrorHandlerMiddleware), statusCode, message ?? error.Message, error.GetType().ToString(), new Dictionary<string, object?>()
            {
                { "env", isDevelopment ? "dev" : "prod" },
                { "version", AppVersionService.Version},
            }).ExecuteAsync(context);
    }
    public static class AppVersionService
    {
        public static readonly string? Version =
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }
}

