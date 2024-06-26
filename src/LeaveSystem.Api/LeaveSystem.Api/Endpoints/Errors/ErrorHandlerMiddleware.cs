using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System.Net;
using static LeaveSystem.Api.Endpoints.Errors.ErrorHandlerMiddleware;
using System.Reflection;

namespace LeaveSystem.Api.Endpoints.Errors;

using Ardalis.GuardClauses;
using FluentValidation;
using Shared.FluentValidation;

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
        catch (NotFoundException e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status404NotFound, e, "Resource not found");
        }
        catch (EntityNotFoundException e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status404NotFound, e, e.Message);
        }
        catch (ArgumentException e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status400BadRequest, e);
        }
        catch (BadHttpRequestException e)
        {
            await HandleExceptionAsync(httpContext, e.StatusCode, e);
        }
        catch (ValidationException e)
        {
            var error = e.Errors.FirstOrDefault();
            if (error is null)
            {
                await HandleExceptionAsync(httpContext, StatusCodes.Status500InternalServerError, e,
                    "Error while handling ValidationException.");
                return;
            }

            var statusCode = error.ErrorCode switch
            {
                ValidationErrorCodes.ArgumentOutOfRange => StatusCodes.Status416RequestedRangeNotSatisfiable,
                ValidationErrorCodes.Argument => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
            await HandleExceptionAsync(httpContext, statusCode, e, error.ErrorMessage);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException e)
        {
            await HandleExceptionAsync(httpContext, StatusCodes.Status400BadRequest, e, e.Message);
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

