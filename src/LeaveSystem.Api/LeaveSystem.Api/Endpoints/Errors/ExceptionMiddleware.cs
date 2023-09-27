using System.Net;
using System.Text.Json;
using LeaveSystem.Api.Extensions;

namespace LeaveSystem.Api.Endpoints.Errors;

public class ExceptionMiddleware
{
    private readonly RequestDelegate next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (ArgumentOutOfRangeException)
        {
            await httpContext.HandleExceptionAsync(HttpStatusCode.RequestedRangeNotSatisfiable);
        }
        catch (GoldenEye.Exceptions.NotFoundException)
        {
            await httpContext.HandleExceptionAsync(HttpStatusCode.NotFound, "Resource not found");
        }
        catch (Exception e) when (e is ArgumentException or ArgumentNullException)
        {
            await httpContext.HandleExceptionAsync(HttpStatusCode.BadRequest);
        }
        catch (Exception)
        {
            await httpContext.HandleExceptionAsync(HttpStatusCode.InternalServerError,
                "Internal Server Error from the custom middleware.");
        }
    }
}

