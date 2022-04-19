using LeaveSystem.Services.LeaveType;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api
{
    public static class LeaveTypeEndpoints
    {
        private const int MaxPageSize = 25;
        public static IEndpointRouteBuilder MapLeaveTypeEndpoints(this IEndpointRouteBuilder builder, string scopeRequiredByApi)
        {
            builder.MapPost("/leavetype", async (HttpContext httpContext, CreateLeaveType createRequest, LeaveTypeService leaveTypeService, CancellationToken cancellation) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

                await leaveTypeService.Create(createRequest, cancellation);

                return Results.NoContent();
            })
            .WithName("CreateLeaveType")
            .RequireAuthorization();

            builder.MapGet("/leavetype", (HttpContext httpContext, int? page, int? pageSize, LeaveTypeService leaveTypeService, CancellationToken cancellation) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
                var pageSizeShrinked = pageSize.HasValue ?
                    (pageSize.Value > MaxPageSize ? MaxPageSize : pageSize.Value) :
                    MaxPageSize;
                return leaveTypeService.GetAll(page, pageSizeShrinked, cancellation);
            })
            .WithName("GetAllLeaveTypes")
            .RequireAuthorization();

            builder.MapGet("/leavetype/{leaveTypeId}", (HttpContext httpContext, Guid leaveTypeId, LeaveTypeService leaveTypeService, CancellationToken cancellation) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

                return leaveTypeService.Get(leaveTypeId, cancellation);
            })
            .WithName("GetLeaveTypeById")
            .RequireAuthorization();

            builder.MapPut("/leavetype", (HttpContext httpContext, UpdateLeaveType updateRequest, LeaveTypeService leaveTypeService, CancellationToken cancellation) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

                return leaveTypeService.Update(updateRequest, cancellation);
            })
            .WithName("UpdateLeaveType")
            .RequireAuthorization();

            builder.MapDelete("/leavetype/{leaveTypeId}", (HttpContext httpContext, Guid leaveTypeId, LeaveTypeService leaveTypeService, CancellationToken cancellation) =>
            {
                httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

                return leaveTypeService.Remove(leaveTypeId, cancellation);
            })
            .WithName("RemoveLeaveType")
            .RequireAuthorization();

            return builder;
        }
    }
}
