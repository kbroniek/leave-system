using Ardalis.GuardClauses;
using GoldenEye.Queries;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.Shared;
using Marten.Pagination;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.WorkingHours;

public static class WorkingHoursEndpoints
{
    public const string GetWorkingHoursEndpointsPolicyName = "GetWorkingHours";
    public const string GetUserWorkingHoursEndpointsPolicyName = "GetUserWorkingHours";
    public const string GetUserWorkingHoursDurationEndpointsPolicyName = "GetUserWorkingHoursDuration";

    public static IEndpointRouteBuilder AddWorkingHoursEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/workingHours",
                async (HttpContext httpContext, IQueryBus queryBus, GetWorkingHoursQuery query,
                    CancellationToken cancellationToken) =>
                {
                    httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

                    var workingHours =
                        await queryBus.Send<GetWorkingHours, IPagedList<EventSourcing.WorkingHours.WorkingHours>>(
                            GetWorkingHours.Create(
                                query.PageSize,
                                query.PageNumber,
                                query.DateFrom,
                                query.DateTo,
                                query.UserIds,
                                httpContext.User.CreateModel(),
                                query.Statuses
                            ), cancellationToken);
                    return PagedListResponse.From(workingHours);
                })
            .WithName(GetWorkingHoursEndpointsPolicyName)
            .RequireAuthorization(GetWorkingHoursEndpointsPolicyName);

        endpoint.MapGet("api/workingHours/{userId}", async (HttpContext httpContext, IQueryBus queryBus, string? userId, CancellationToken cancellationToken) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
                Guard.Against.Nill(userId);

                return await queryBus.Send<GetCurrentWorkingHoursByUserId, EventSourcing.WorkingHours.WorkingHours>(
                    GetCurrentWorkingHoursByUserId.Create(userId), cancellationToken
                );
            })
            .WithName(GetUserWorkingHoursEndpointsPolicyName)
            .RequireAuthorization(GetUserWorkingHoursEndpointsPolicyName);

        return endpoint;
    }
}