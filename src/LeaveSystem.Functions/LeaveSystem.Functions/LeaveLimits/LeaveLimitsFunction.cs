namespace LeaveSystem.Functions.LeaveLimits;

using LeaveSystem.Functions.Extensions;
using LeaveSystem.Functions.LeaveRequests;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

public class LeaveLimitsFunction
{
    private readonly ILogger<LeaveLimitsFunction> logger;

    public LeaveLimitsFunction(ILogger<LeaveLimitsFunction> logger)
    {
        this.logger = logger;
    }

    [Function(nameof(GetUserLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)}")]
    public IActionResult GetUserLeaveLimits([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leavelimits/user")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return GetLeaveLimits(req);
    }

    [Function(nameof(GetLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public IActionResult GetLeaveLimits([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leavelimits")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var getLeaveLimitQuery = req.HttpContext.BindGetLeaveLimitQuery();
        var userId = req.HttpContext.GetUserId();
        var limits = new[] { CreateLimit(getLeaveLimitQuery.DateFrom, getLeaveLimitQuery.DateFrom, userId) };
        return new OkObjectResult(limits.ToPagedListResponse());
    }

    [Function(nameof(GetLeaveLimit))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public IActionResult GetLeaveLimit([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leavelimits/{leaveLimitId:guid}")] HttpRequest req, Guid leaveLimitId)
    {
        var userId = req.HttpContext.GetUserId();
        var limit = CreateLimit(null, null, userId);
        return new OkObjectResult(limit);
    }

    [Function(nameof(CreateLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public IActionResult CreateLeaveLimits([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leavelimits")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var userId = req.HttpContext.GetUserId();
        var limit = CreateLimit(null, null, userId);
        return new CreatedResult($"/leavelimits/{limit.Id}", limit);
    }

    [Function(nameof(UpdateLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public IActionResult UpdateLeaveLimits([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leavelimits")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var userId = req.HttpContext.GetUserId();
        var limit = CreateLimit(null, null, userId);
        return new OkObjectResult(limit);
    }

    [Function(nameof(RemoveLeaveLimit))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public IActionResult RemoveLeaveLimit([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "delete",
        Route = "leavelimits/{leaveLimitId:guid}")] HttpRequest req, Guid leaveLimitId)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new NoContentResult();
    }

    private static LeaveLimitDto CreateLimit(DateOnly? dateFrom, DateOnly? dateTo, string userId) => new LeaveLimitDto(
            Guid.Parse("3b8a8a97-992f-4965-abf8-fe5a9cf91862"),
            TimeSpan.FromHours(8) * 26,
            TimeSpan.FromHours(8) * 2,
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            dateFrom ?? DateOnly.Parse("2024-01-01"),
            dateTo ?? DateOnly.Parse("2024-12-31"),
            userId);
}
