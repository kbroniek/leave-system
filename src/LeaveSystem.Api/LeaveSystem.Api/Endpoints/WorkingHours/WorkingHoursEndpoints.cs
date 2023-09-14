using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Queries;
using LeaveSystem.Api.Extensions;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using Marten.Pagination;
using Microsoft.Identity.Web.Resource;
using NotFoundException = GoldenEye.Exceptions.NotFoundException;

namespace LeaveSystem.Api.Endpoints.WorkingHours;

public static class WorkingHoursEndpoints
{
    public const string GetWorkingHoursEndpointsPolicyName = "GetWorkingHours";
    public const string GetUserWorkingHoursEndpointsPolicyName = "GetUserWorkingHours";
    public const string GetUserWorkingHoursDurationEndpointsPolicyName = "GetUserWorkingHoursDuration";
    public const string AddUserWorkingHoursPolicyName = "AddWorkingHours";

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

        endpoint.MapGet("api/workingHours/{userId}", async (HttpContext httpContext, IQueryBus queryBus, string? userId,
                CancellationToken cancellationToken) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
                Guard.Against.Nill(userId);

                try
                {
                    var workingHours =
                        await queryBus.Send<GetCurrentWorkingHoursByUserId, EventSourcing.WorkingHours.WorkingHours>(
                            GetCurrentWorkingHoursByUserId.Create(userId), cancellationToken
                        );
                    return Results.Json(workingHours.ToDto());
                }
                catch (NotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName(GetUserWorkingHoursEndpointsPolicyName)
            .RequireAuthorization(GetUserWorkingHoursEndpointsPolicyName);

        endpoint.MapPost("api/workingHours",
                async (HttpContext httpContext, ICommandBus commandBus, AddWorkingHoursDto addWorkingHoursDto,
                    CancellationToken cancellationToken) =>
                {
                    httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
                    var workingHoursId = Guid.NewGuid();
                    var command = AddWorkingHours.Create(
                        workingHoursId,
                        addWorkingHoursDto.UserId,
                        addWorkingHoursDto.DateFrom,
                        addWorkingHoursDto.DateTo,
                        addWorkingHoursDto.Duration,
                        addWorkingHoursDto.AddedBy
                    );
                    await commandBus.Send(command, cancellationToken);
                    return Results.Created("api/workingHours", workingHoursId);
                })
            .WithName(AddUserWorkingHoursPolicyName)
            .RequireAuthorization(AddUserWorkingHoursPolicyName);

        return endpoint;
    }
}