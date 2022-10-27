using Ardalis.GuardClauses;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.WorkingHours;

public static class WorkingHoursEndpoints
{
    public const string GetWorkingHoursEndpointsPolicyName = "GetWorkingHours";
    public const string GetUserWorkingHoursEndpointsPolicyName = "GetUserWorkingHours";
    public const string GetUserWorkingHoursDurationEndpointsPolicyName = "GetUserWorkingHoursDuration";
    public static IEndpointRouteBuilder AddWorkingHoursEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/workingHours", async (HttpContext httpContext, WorkingHoursService service, GetWorkingHoursQuery query, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var workingHours = await service.GetUserWorkingHours(
                query.UserEmails,
                query.DateFrom,
                query.DateTo,
                cancellationToken);
            return new
            {
                WorkingHours = workingHours,
            };
        })
        .WithName(GetWorkingHoursEndpointsPolicyName)
        .RequireAuthorization(GetWorkingHoursEndpointsPolicyName);

        endpoint.MapGet("api/workingHours/{userEmail}", async (HttpContext httpContext, WorkingHoursService service, string? userEmail, GetUserWorkingHoursQuery query, CancellationToken cancellationToken) =>
        {
            //httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
            Guard.Against.InvalidEmail(userEmail);

            var workingHours = await service.GetUserWorkingHours(
                new string[] { userEmail },
                query.DateFrom,
                query.DateTo,
                cancellationToken);
            return new
            {
                WorkingHours = workingHours,
            };
        })
        .WithName(GetUserWorkingHoursEndpointsPolicyName)
        .RequireAuthorization(GetUserWorkingHoursEndpointsPolicyName);

        endpoint.MapGet("api/workingHours/{userEmail}/duration", async (HttpContext httpContext, WorkingHoursService service, string? userEmail, GetUserWorkingHoursQuery query, CancellationToken cancellationToken) =>
        {
            //httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            Guard.Against.InvalidEmail(userEmail);
            var duration = await service.GetUserSingleWorkingHoursDuration(
                userEmail,
                query.DateFrom,
                query.DateTo,
                cancellationToken);
            return new
            {
                Duration = duration,
            };
        })
        .WithName(GetUserWorkingHoursDurationEndpointsPolicyName)
        .RequireAuthorization(GetUserWorkingHoursDurationEndpointsPolicyName);

        return endpoint;
    }
}
