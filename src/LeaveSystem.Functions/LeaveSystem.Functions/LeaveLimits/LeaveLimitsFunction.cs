namespace LeaveSystem.Functions.LeaveLimits;

using System.Threading;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class LeaveLimitsFunction(
    SearchLeaveLimitsRepository searchRepository,
    CreateLeaveLimitsValidator createValidator,
    ILogger<LeaveLimitsFunction> logger)
{
    [Function(nameof(SearchUserLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> SearchUserLeaveLimits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leavelimits/user")] HttpRequest req, CancellationToken cancellationToken)
    {
        var userId = req.HttpContext.GetUserId();
        var query = req.HttpContext.BindSearchUserLeaveLimitQuery();
        if (query.IsFailure)
        {
            return query.Error.ToObjectResult("Error occurred while getting a leave limits.");
        }
        var result = await searchRepository.GetLimits(query.Value.Year, [userId], query.Value.LeaveTypeIds, query.Value.PageSize, query.Value.ContinuationToken, cancellationToken);

        return result.Match<IActionResult>(
            leaveLimits => new OkObjectResult(leaveLimits.limits.ToPagedListResponse(leaveLimits.continuationToken)),
            error => error.ToObjectResult("Error occurred while getting a leave limits."));
    }

    [Function(nameof(SearchLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public async Task<IActionResult> SearchLeaveLimits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leavelimits")] HttpRequest req, CancellationToken cancellationToken)
    {
        var query = req.HttpContext.BindSearchLeaveLimitQuery();
        if (query.IsFailure)
        {
            return query.Error.ToObjectResult("Error occurred while getting a leave limits.");
        }
        var result = await searchRepository.GetLimits(query.Value.Year, query.Value.UserIds, query.Value.LeaveTypeIds, query.Value.PageSize, query.Value.ContinuationToken, cancellationToken);

        return result.Match<IActionResult>(
            leaveLimits => new OkObjectResult(leaveLimits.limits.ToPagedListResponse(leaveLimits.continuationToken)),
            error => error.ToObjectResult("Error occurred while getting a leave limits."));
    }

    [Function(nameof(CreateLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public async Task<LeaveLimitOutput> CreateLeaveLimits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leavelimits")] HttpRequest req,
        [FromBody] LeaveLimitDto leaveLimit)
    {
        var result = await createValidator.Validate(leaveLimit);
        if (result.IsFailure)
        {
            return new()
            {
                Result = result.Error.ToObjectResult($"Error occurred while creating a leave type. LeaveLimitId = {leaveLimit.Id}.")
            };
        }

        return new()
        {
            Result = new CreatedResult($"/leavetypes/{leaveLimit.Id}", leaveLimit),
            LeaveLimit = leaveLimit
        };
    }

    [Function(nameof(UpdateLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public IActionResult UpdateLeaveLimits([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leavelimits/{leaveLimitId:guid}")] HttpRequest req, Guid leaveLimitId)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var userId = req.HttpContext.GetUserId();
        var limit = CreateLimit(null, null, userId);
        return new OkObjectResult(limit);
    }

    [Function(nameof(RemoveLeaveLimit))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
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


    public class LeaveLimitOutput
    {
        [HttpResult]
        public IActionResult Result { get; set; }

        [CosmosDBOutput("%DatabaseName%", "%LeaveLimitsContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public LeaveLimitDto? LeaveLimit { get; set; }
    }
}
