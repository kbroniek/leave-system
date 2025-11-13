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
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class LeaveLimitsFunction(
    SearchLeaveLimitsRepository searchRepository,
    CreateLeaveLimitsValidator createValidator,)
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
            return new(new Error($"{nameof(LeaveLimitDto.Id)} cannot be different than {nameof(leaveLimitId)}.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_ID_MISMATCH)
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

    [Function(nameof(GenerateNewYearLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public async Task<IActionResult> GenerateNewYearLimits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leavelimits/generate-new-year")] HttpRequest req, CancellationToken cancellationToken)
    {
        var yearParam = req.Query["year"].FirstOrDefault();
        if (!int.TryParse(yearParam, out var templateYear))
        {
            return new BadRequestObjectResult(new { error = "Invalid year parameter. Please provide a valid year." });
        }

        var templatesResult = await searchRepository.GetLimitTemplatesForNewYear(templateYear, cancellationToken);
        if (templatesResult.IsFailure)
        {
            return templatesResult.Error.ToObjectResult("Error occurred while retrieving limit templates.");
        }

        var templates = templatesResult.Value;
        var newYear = templateYear + 1;
        var newYearLimits = new List<LeaveLimitDto>();

        foreach (var template in templates)
        {
            var newLimit = template with
            {
                Id = Guid.NewGuid(),
                ValidSince = template.ValidSince.HasValue ? new DateOnly(newYear, template.ValidSince.Value.Month, template.ValidSince.Value.Day) : new DateOnly(newYear, 1, 1),
                ValidUntil = template.ValidUntil.HasValue ? new DateOnly(newYear, template.ValidUntil.Value.Month, template.ValidUntil.Value.Day) : new DateOnly(newYear, 12, 31)
            };
            newYearLimits.Add(newLimit);
        }

        return new OkObjectResult(new { items = newYearLimits });
    }

    [Function(nameof(BatchInsertLimits))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public async Task<BatchLeaveLimitOutput> BatchInsertLimits(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leavelimits/batch")] HttpRequest req,
        [FromBody] List<LeaveLimitDto> leaveLimits, CancellationToken cancellationToken)
    {
        var validatedLimits = new List<LeaveLimitDto>();
        var errors = new List<string>();

        foreach (var limit in leaveLimits)
        {
            var validationResult = await createValidator.Validate(
                limit.ValidSince, limit.ValidUntil, limit.AssignedToUserId, limit.LeaveTypeId, limit.Id, cancellationToken);

            if (validationResult.IsFailure)
            {
                errors.Add($"Limit {limit.Id}: {validationResult.Error.Message}");
            }
            else
            {
                validatedLimits.Add(limit);
            }
        }

        if (errors.Count != 0)
        {
            return new(new BadRequestObjectResult(new { errors }));
        }

        return new(new CreatedResult("/leavelimits/batch", new { items = validatedLimits, count = validatedLimits.Count }), validatedLimits);
    }

    public record LeaveLimitOutput(
        [property: HttpResult] IActionResult Result,
        [property: CosmosDBOutput("%DatabaseName%", "%LeaveLimitsContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)] LeaveLimitDto? LeaveLimit = null);

    public record BatchLeaveLimitOutput(
        [property: HttpResult] IActionResult Result,
        [property: CosmosDBOutput("%DatabaseName%", "%LeaveLimitsContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)] List<LeaveLimitDto>? LeaveLimits = null);
}
