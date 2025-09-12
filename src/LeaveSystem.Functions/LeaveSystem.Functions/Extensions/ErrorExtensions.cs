namespace LeaveSystem.Functions.Extensions;

using System.Net;
using LeaveSystem.Domain;
using Microsoft.AspNetCore.Mvc;

internal static class ErrorExtensions
{
    internal static ObjectResult ToObjectResult(this Error error, string title, string? instance = null, string? type = null)
    {
        var httpStatusCode = error.HttpStatusCode == 0 ? HttpStatusCode.InternalServerError : error.HttpStatusCode;
        return new(new ProblemDetails
        {
            Title = title,
            Detail = error.Message,
            Status = (int)httpStatusCode,
            Instance = instance,
            Type = type,
            Extensions = { { "errorCode", error.ErrorCode } }
        })
        {
            StatusCode = (int)httpStatusCode
        };
    }
}
