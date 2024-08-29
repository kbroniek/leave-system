namespace LeaveSystem.Functions.Extensions;
using LeaveSystem.Domain;
using Microsoft.AspNetCore.Mvc;

internal static class ErrorExtensions
{
    internal static ObjectResult ToObjectResult(this Error error, string title, string? instance = null, string? type = null) =>
        new(new ProblemDetails
        {
            Title = title,
            Detail = error.Message,
            Status = (int)error.HttpStatusCode,
            Instance = instance,
            Type = type
        })
        {
            StatusCode = (int)error.HttpStatusCode
        };
}
