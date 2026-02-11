namespace LeaveSystem.Functions.LeaveLimits;

using System.Threading;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveLimits;
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
    CreateLeaveLimitsValidator createValidator,
    GenerateNewYearLimitsService generateNewYearLimitsService,
    LeaveRequestEventsRepository eventsRepository)
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
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.LeaveLimitAdmin)},{nameof(RoleType.HumanResource)}")]
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leavelimits/generate-new-year/{templateYear:int}")] HttpRequest req,
        int templateYear,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveTypesContainerName%",
            Connection  = "CosmosDBConnection",
            SqlQuery = "SELECT c.id FROM c WHERE c.properties.catalog = 'Saturday'"
            )] IEnumerable<LeaveTypeDetailsDto> saturdayLeaveTypes,
        CancellationToken cancellationToken)
    {
        // Fetch all necessary data at the beginning of the function
        var saturdayLeaveTypeIds = saturdayLeaveTypes.Select(t => t.Id).ToArray();

        // 1. Get template limits for the year (excluding Saturday leave types)
        var templatesResult = await searchRepository.GetLimitTemplatesForNewYear(
            templateYear,
            saturdayLeaveTypeIds,
            cancellationToken);

        if (templatesResult.IsFailure)
        {
            return templatesResult.Error.ToObjectResult("Error occurred while retrieving limit templates.");
        }

        // 2. Get all pending leave request events for the template year
        var pendingEventsResult = await eventsRepository.GetPendingEventsForYear(templateYear, cancellationToken);
        if (pendingEventsResult.IsFailure)
        {
            return pendingEventsResult.Error.ToObjectResult("Error occurred while retrieving pending events.");
        }

        var pendingEvents = pendingEventsResult.Value;

        // 3. Get cancelled stream IDs
        var streamIds = pendingEvents.Select(e => e.StreamId).ToList();
        var cancelledStreamIdsResult = await eventsRepository.GetCancelledStreamIds(streamIds, cancellationToken);
        if (cancelledStreamIdsResult.IsFailure)
        {
            return cancelledStreamIdsResult.Error.ToObjectResult("Error occurred while retrieving cancelled events.");
        }

        var cancelledStreamIds = cancelledStreamIdsResult.Value;

        // All calculations are done in the domain service
        var saturdayLeaveTypeId = saturdayLeaveTypes.FirstOrDefault()?.Id;
        var newYearLimits = generateNewYearLimitsService.GenerateNewYearLimits(
            templatesResult.Value,
            pendingEvents,
            cancelledStreamIds,
            templateYear,
            saturdayLeaveTypeId);

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
