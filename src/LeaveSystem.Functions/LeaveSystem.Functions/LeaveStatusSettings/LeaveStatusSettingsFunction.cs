namespace LeaveSystem.Functions.LeaveStatusSettings;

using System.Drawing;
using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class LeaveStatusSettingsFunction(ILogger<LeaveStatusSettingsFunction> logger)
{
    [Function(nameof(GetLeaveStatusesSettings))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)}")]
    public async Task<IActionResult> GetLeaveStatusesSettings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "settings/leavestatus")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveStatusSettingsContainerName%",
            SqlQuery = "SELECT * FROM c WHERE c.state = 'Active' OR NOT IS_DEFINED(c.state)",
            Connection  = "CosmosDBConnection")] IEnumerable<LeaveStatusSettingsDto> leaveTypes) =>
        new OkObjectResult(leaveTypes.ToPagedListResponse());

    [Function(nameof(GetLeaveStatusSettings))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> GetLeaveStatusSettings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "settings/leavestatus/{leaveStatusSettingsId:guid}")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveStatusSettingsContainerName%",
            Connection  = "CosmosDBConnection",
            Id = "{leaveStatusSettingsId}",
            PartitionKey = "{leaveStatusSettingsId}")] LeaveStatusSettingsDto leaveType) =>
        new OkObjectResult(leaveType);

    [Function(nameof(CreateLeaveStatusSettings))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public LeaveTypeOutput CreateLeaveStatusSettings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "settings/leavestatus")] HttpRequest req,
        [FromBody] LeaveStatusSettingsDto leaveStatusSettings)
    {
        var validateResult = Validate(leaveStatusSettings);
        return validateResult.Match<LeaveTypeOutput>(
            () => new()
            {
                Result = new CreatedResult($"/settings/leavestatus/{leaveStatusSettings.Id}", leaveStatusSettings),
                LeaveStatusSettings = leaveStatusSettings
            },
            error => new()
            {
                Result = error.ToObjectResult($"Error occurred while updating a leave status settings. LeaveStatusSettingsId = {leaveStatusSettings.Id}.")
            }
        );
    }

    [Function(nameof(UpdateLeaveStatusSettings))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public LeaveTypeOutput UpdateLeaveStatusSettings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "settings/leavestatus/{leaveStatusSettingsId:guid}")] HttpRequest req,
        Guid leaveStatusSettingsId, [FromBody] LeaveStatusSettingsDto leaveStatusSettings)
    {
        var validateResult = Validate(leaveStatusSettingsId, leaveStatusSettings);
        return validateResult.Match<LeaveTypeOutput>(
            () => new()
            {
                Result = new CreatedResult($"/settings/leavestatus/{leaveStatusSettings.Id}", leaveStatusSettings),
                LeaveStatusSettings = leaveStatusSettings
            },
            error => new()
            {
                Result = error.ToObjectResult($"Error occurred while updating a leave status settings. LeaveStatusSettingsId = {leaveStatusSettings.Id}.")
            }
        );
    }

    [Function(nameof(RemoveLeaveStatusSettings))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public LeaveTypeOutput RemoveLeaveStatusSettings(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "settings/leavestatus/{leaveTypeId:guid}")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveStatusSettingsContainerName%",
            Connection  = "CosmosDBConnection",
            Id = "{leaveTypeId}",
            PartitionKey = "{leaveTypeId}")] LeaveStatusSettingsDto leaveStatusSettingsFromDb)
    {
        var deletedLeaveStatusSettings = leaveStatusSettingsFromDb with { State = LeaveStatusSettingsDto.LeaveStatusSettingsState.Inactive };
        return new()
        {
            Result = new NoContentResult(),
            LeaveStatusSettings = deletedLeaveStatusSettings
        };
    }

    private Result<Error> Validate(Guid leaveStatusSettingsId, LeaveStatusSettingsDto leaveStatusSettings)
    {
        if (leaveStatusSettingsId != leaveStatusSettings.Id)
        {
            return new Error($"{nameof(LeaveStatusSettingsDto.Id)} cannot be different than LeaveStatusSettingsId.", System.Net.HttpStatusCode.BadRequest);
        }
        return Validate(leaveStatusSettings);
    }
    private Result<Error> Validate(LeaveStatusSettingsDto leaveStatusSettings)
    {
        try
        {
            if (new ColorConverter().ConvertFromString(leaveStatusSettings.Color) is not Color)
            {
                return new Error($"{nameof(LeaveStatusSettingsDto.Color)} is not valid hex color.", System.Net.HttpStatusCode.BadRequest);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{Message}", $"{nameof(LeaveStatusSettingsDto.Color)} is not valid hex color.");
            return new Error($"{nameof(LeaveStatusSettingsDto.Color)} is not valid hex color.", System.Net.HttpStatusCode.BadRequest);
        }
        return Result.Default;
    }

    public class LeaveTypeOutput
    {
        [HttpResult]
        public IActionResult Result { get; set; }

        [CosmosDBOutput("%DatabaseName%", "%LeaveStatusSettingsContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public LeaveStatusSettingsDto? LeaveStatusSettings { get; set; }
    }
}
