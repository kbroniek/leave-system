namespace LeaveSystem.Functions.Extensions;
using System.Security.Claims;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;

internal static class UserExtensions
{
    public static string GetUserId(this HttpContext context) =>
        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new NotFoundException(nameof(HttpContent), ClaimTypes.NameIdentifier);
}
