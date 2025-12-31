using Microsoft.AspNetCore.Authentication;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions.Auth;

public class RoleClaimsTransformation(IRolesRepository rolesRepository, ILogger<RoleClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if user is authenticated
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        // Check if roles have already been added
        if (principal.HasClaim(ClaimTypes.Role, nameof(RoleType.GlobalAdmin)) ||
            principal.Claims.Any(c => c.Type == ClaimTypes.Role && Enum.GetNames<RoleType>().Contains(c.Value)))
        {
            return principal;
        }

        try
        {
            var userModel = principal.CreateModel();
            if (string.IsNullOrEmpty(userModel.Id))
            {
                logger.LogWarning("User ID is missing from claims");
                return principal;
            }

            // Fetch roles from CosmosDB
            var rolesResult = await rolesRepository.GetUserRoles(userModel.Id, CancellationToken.None);
            if (rolesResult.IsFailure)
            {
                logger.LogWarning("Failed to retrieve user roles for user {UserId}: {Error}", userModel.Id, rolesResult.Error.Message);
                return principal;
            }

            // Create new identity with role claims
            var identity = new ClaimsIdentity(principal.Identity);

            // Add all user roles as claims
            foreach (var role in rolesResult.Value.Roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while transforming claims for user");
            return principal;
        }
    }
}
