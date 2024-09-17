namespace LeaveSystem.Functions.LeaveLimits;

using System.Threading;
using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Functions.LeaveLimits.Repositories;
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
        [FromBody] LeaveLimitDto leaveLimit, CancellationToken cancellationToken)
    {
        var result = await createValidator.Validate(
            leaveLimit.ValidSince, leaveLimit.ValidUntil, leaveLimit.AssignedToUserId, leaveLimit.LeaveTypeId, leaveLimit.Id, cancellationToken);
        if (result.IsFailure)
        {
            return new(result.Error.ToObjectResult($"Error occurred while creating a leave limit. LeaveLimitId = {leaveLimit.Id}."));
        }

        return new(new CreatedResult($"/leavetypes/{leaveLimit.Id}", leaveLimit), leaveLimit);
    }

    [Function(nameof(UpdateLeaveLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public async Task<LeaveLimitOutput> UpdateLeaveLimits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "leavelimits/{leaveLimitId:guid}")] HttpRequest req,
        Guid leaveLimitId, [FromBody] LeaveLimitDto leaveLimit, CancellationToken cancellationToken)
    {
        if (leaveLimitId != leaveLimit.Id)
        {
            return new(new Error($"{nameof(LeaveLimitDto.Id)} cannot be different than {nameof(leaveLimitId)}.", System.Net.HttpStatusCode.BadRequest)
                    .ToObjectResult($"Error occurred while updating a leave type. LeaveLimit = {leaveLimit.Id}."));
        }
        var result = await createValidator.Validate(
            leaveLimit.ValidSince, leaveLimit.ValidUntil, leaveLimit.AssignedToUserId, leaveLimit.LeaveTypeId, leaveLimit.Id, cancellationToken);
        if (result.IsFailure)
        {
            return new(result.Error.ToObjectResult($"Error occurred while updating a leave limit. LeaveLimitId = {leaveLimit.Id}."));
        }

        return new(new OkObjectResult(leaveLimit), leaveLimit);
    }

    [Function(nameof(RemoveLeaveLimit))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public async Task<LeaveLimitOutput> RemoveLeaveLimit(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "leavelimits/{leaveLimitId:guid}")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveLimitsContainerName%",
            Connection  = "CosmosDBConnection",
            Id = "{leaveLimitId}",
            PartitionKey = "{leaveLimitId}")] LeaveLimitDto LeaveLimitFromDb)
    {
        var deletedLeaveLimit = LeaveLimitFromDb with { State = LeaveLimitDto.LeaveLimitState.Inactive };
        return new(new NoContentResult(), deletedLeaveLimit);
    }

    public record LeaveLimitOutput(
        [property: HttpResult] IActionResult Result,
        [property: CosmosDBOutput("%DatabaseName%", "%LeaveLimitsContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)] LeaveLimitDto? LeaveLimit = null);
}
