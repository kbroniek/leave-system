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
                query.UserIds,
                query.DateFrom,
                query.DateTo,
                cancellationToken);
            return new Web.Pages.WorkingHours.WorkingHoursCollectionDto(workingHours);
        })
        .WithName(GetWorkingHoursEndpointsPolicyName)
        .RequireAuthorization(GetWorkingHoursEndpointsPolicyName);

        endpoint.MapGet("api/workingHours/{userId}", async (HttpContext httpContext, WorkingHoursService service, string? userId, GetUserWorkingHoursQuery query, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
            Guard.Against.Nill(userId);

            var workingHours = await service.GetUserWorkingHours(
                new string[] { userId },
                query.DateFrom,
                query.DateTo,
                cancellationToken);
            return new Web.Pages.WorkingHours.WorkingHoursCollectionDto(workingHours);
        })
        .WithName(GetUserWorkingHoursEndpointsPolicyName)
        .RequireAuthorization(GetUserWorkingHoursEndpointsPolicyName);

        endpoint.MapGet("api/workingHours/{userId}/duration", async (HttpContext httpContext, WorkingHoursService service, string? userId, GetUserWorkingHoursQuery query, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            Guard.Against.Nill(userId);
            var duration = await service.GetUserSingleWorkingHoursDuration(
                userId,
                query.DateFrom,
                query.DateTo,
                cancellationToken);
            return new Web.Pages.WorkingHours.WorkingHoursDurationDto(duration);
        })
        .WithName(GetUserWorkingHoursDurationEndpointsPolicyName)
        .RequireAuthorization(GetUserWorkingHoursDurationEndpointsPolicyName);

        return endpoint;
    }
}
