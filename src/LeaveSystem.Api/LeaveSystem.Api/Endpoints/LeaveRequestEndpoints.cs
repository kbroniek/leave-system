using GoldenEye.Commands;
using GoldenEye.Queries;
using LeaveSystem.Api.Responses;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.CancellingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.RejectingLeaveRequest;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints;

public record class GetLeaveRequestsQuery(int? PageNumber, int? PageSize, DateTimeOffset? DateFrom, DateTimeOffset? DateTo,
    Guid[]? LeaveTypeIds, LeaveRequestStatus[]? Statuses, string[]? CreatedByEmails)
{
    public static ValueTask<GetLeaveRequestsQuery?> BindAsync(HttpContext context)
        => ValueTask.FromResult<GetLeaveRequestsQuery?>(new(
            PageNumber: TryParseInt(context.Request.Query, nameof(PageNumber)),
            PageSize: TryParseInt(context.Request.Query, nameof(PageSize)),
            DateFrom: TryParseDateTimeOffset(context.Request.Query, nameof(DateFrom)),
            DateTo: TryParseDateTimeOffset(context.Request.Query, nameof(DateTo)),
            LeaveTypeIds: TryParseGuids(context.Request.Query, nameof(LeaveTypeIds)),
            Statuses: TryParseLeaveRequestStatuses(context.Request.Query, nameof(Statuses)),
            CreatedByEmails: TryParseStrings(context.Request.Query, nameof(CreatedByEmails))
            ));

    private static string[]? TryParseStrings(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? paramValue.ToArray() : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to string type.", ex);
        }
    }

    private static LeaveRequestStatus[]? TryParseLeaveRequestStatuses(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? paramValue.Select(x => Enum.Parse<LeaveRequestStatus>(x)).ToArray() : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to LeaveRequestStatus type.", ex);
        }
    }

    private static Guid[]? TryParseGuids(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? paramValue.Select(x => Guid.Parse(x)).ToArray() : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to Guid type.", ex);
        }
    }

    private static DateTimeOffset? TryParseDateTimeOffset(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? DateTimeOffset.Parse(paramValue) : null;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to DateTimeOffset type.", ex);
        }
    }

    private static int? TryParseInt(IQueryCollection query, string paramName)
    {
        var paramValue = query[paramName];
        try
        {
            return !string.IsNullOrEmpty(paramValue) ? int.Parse(paramValue) : null;
        }
        catch(Exception ex)
        {
            throw new FormatException($"Cannot parse the {paramName} query parameter to int.", ex);
        }
    }
}

public static class LeaveRequestEndpoints
{
    public const string GetLeaveRequestsName = "GetLeaveRequests";
    public const string CreateLeaveRequestName = "CreateLeaveRequest";
    public const string AcceptLeaveRequestName = "AcceptLeaveRequest";
    public const string RejectLeaveRequestName = "RejectLeaveRequest";
    public const string CancelLeaveRequestName = "CancelLeaveRequest";
    public static void AddLeaveRequestEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/leaveRequests", async (HttpContext httpContext, IQueryBus queryBus, GetLeaveRequestsQuery query, CancellationToken cancellationToken) =>
        {
            var pagedList = await queryBus.Send<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>(GetLeaveRequests.Create(
                query.PageNumber,
                query.PageSize,
                query.DateFrom,
                query.DateTo,
                query.LeaveTypeIds,
                query.Statuses,
                query.CreatedByEmails,
                httpContext.User.CreateModel()), cancellationToken);

            return PagedListResponse.From(pagedList);
        })
        .WithName(GetLeaveRequestsName)
        .RequireAuthorization(GetLeaveRequestsName);

        endpoint.MapPost("api/leaveRequests", async (HttpContext httpContext, ICommandBus commandBus, CreateLeaveRequestDto createLeaveRequest, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var leaveRequestId = Guid.NewGuid();
            var command = EventSourcing.LeaveRequests.CreatingLeaveRequest.CreateLeaveRequest.Create(
                leaveRequestId,
                createLeaveRequest.DateFrom,
                createLeaveRequest.DateTo,
                createLeaveRequest.Duration,
                createLeaveRequest.LeaveTypeId,
                createLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command);
            return Results.Created("api/LeaveRequests", leaveRequestId, cancellationToken);
        })
        .WithName(CreateLeaveRequestName)
        .RequireAuthorization(CreateLeaveRequestName);

        endpoint.MapPut("api/leaveRequests/{id}/accept", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, AcceptLeaveRequestDto acceptLeaveRequest, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.AcceptingLeaveRequest.AcceptLeaveRequest.Create(
                id,
                acceptLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .WithName(AcceptLeaveRequestName)
        .RequireAuthorization(AcceptLeaveRequestName);

        endpoint.MapPut("api/leaveRequests/{id}/reject", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, RejectLeaveRequestDto rejectLeaveRequest, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.RejectingLeaveRequest.RejectLeaveRequest.Create(
                id,
                rejectLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .WithName(RejectLeaveRequestName)
        .RequireAuthorization(RejectLeaveRequestName);

        endpoint.MapDelete("api/leaveRequests/{id}/cancel", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, [FromBody] CancelLeaveRequestDto cancelLeaveRequest, CancellationToken cancellationToken) =>        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.CancelingLeaveRequest.CancelLeaveRequest.Create(
                id,
                cancelLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .WithName(CancelLeaveRequestName)
        .RequireAuthorization(CancelLeaveRequestName);
    }
}
