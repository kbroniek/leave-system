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
        user.FindFirst(ClaimsPrincipalExtensions.IdClaim)?.Value ?? throw new NotFoundException(nameof(HttpContent), ClaimsPrincipalExtensions.IdClaim);
}
