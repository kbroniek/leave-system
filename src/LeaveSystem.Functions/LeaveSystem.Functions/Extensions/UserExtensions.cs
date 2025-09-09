namespace LeaveSystem.Functions.Extensions;
using System.Security.Claims;
using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Http;

internal static class UserExtensions
{
    public static LeaveRequestUserDto MapToLeaveRequestUser(this FederatedUser user) =>
        new(user.Id, user.Name ?? user.Email);

    public static string GetUserId(this HttpContext context) => context.User.GetUserId();
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst("preferred_username")?.Value ?? throw new NotFoundException(nameof(HttpContent), "preferred_username");
}
