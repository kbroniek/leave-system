using GoldenEye.Queries;
using LeaveSystem.EventSourcing.UserLeaveSummary.GettingUserLeaveSummary;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints;

public static class UserLeaveSummaryEndpoints
{
        public const string GetUserLeaveSummary = "GetUserLeaveSummary";
        public static void AddUserLeaveSummaryEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
        {
            endpoint.MapGet("api/userLeaveSummary", async (HttpContext httpContext, IQueryBus queryBus, UserLeaveSummaryQuery query, CancellationToken cancellationToken) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

                var result = await queryBus.Send<GetUserLeaveSummary, UserLeaveSummaryInfo>(
                    EventSourcing.UserLeaveSummary.GettingUserLeaveSummary.GetUserLeaveSummary.Create(
                        requestedBy: httpContext.User.CreateModel(),
                        dateFrom: query.DateFrom,
                        dateTo: query.DateTo
                    ), cancellationToken);

                return Results.Ok(result);
            })
            .WithName(GetUserLeaveSummary)
            .RequireAuthorization(GetUserLeaveSummary);
        }
        }

