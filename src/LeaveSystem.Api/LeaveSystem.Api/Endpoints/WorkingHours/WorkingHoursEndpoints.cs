using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Queries;
using LeaveSystem.Api.Extensions;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;
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
    public const string CreateWorkingHoursPolicyName = "AddWorkingHours";
    public const string ModifyUserWorkingHoursPolicyName = "ModifyUserWorkingHours";

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
                    var user = httpContext.User.CreateModel();
                    var workingHoursId = Guid.NewGuid();
                    var command = CreateWorkingHours.Create(
                        workingHoursId,
                        addWorkingHoursDto.UserId,
                        addWorkingHoursDto.DateFrom,
                        addWorkingHoursDto.DateTo,
                        addWorkingHoursDto.Duration,
                        user
                    );
                    await commandBus.Send(command, cancellationToken);
                    return Results.Created("api/workingHours", new WorkingHoursDto(
                        addWorkingHoursDto.UserId,
                        addWorkingHoursDto.DateFrom.Value,
                        addWorkingHoursDto.DateTo,
                        addWorkingHoursDto.Duration.Value,
                        workingHoursId));
                })
            .WithName(CreateWorkingHoursPolicyName)
            .RequireAuthorization(CreateWorkingHoursPolicyName);

        endpoint.MapPut("api/workingHours/{id}/modify",
                async (HttpContext httpContext, ICommandBus commandBus, ModifyWorkingHoursDto addWorkingHoursDto,
                    CancellationToken cancellationToken, Guid? id) =>
                {
                    httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
                    var user = httpContext.User.CreateModel();
                    var command = ModifyWorkingHours.Create(
                        id,
                        addWorkingHoursDto.UserId,
                        addWorkingHoursDto.DateFrom,
                        addWorkingHoursDto.DateTo,
                        addWorkingHoursDto.Duration,
                        user
                    );
                    await commandBus.Send(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithName(ModifyUserWorkingHoursPolicyName)
            .RequireAuthorization(ModifyUserWorkingHoursPolicyName);

        return endpoint;
    }
}