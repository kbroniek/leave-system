namespace LeaveSystem.Functions.LeaveTypes;

using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class LeaveTypesFunction
{
    [Function(nameof(GetLeaveTypes))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)}")]
    public IActionResult GetLeaveTypes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leavetypes")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveTypesContainerName%",
            SqlQuery = "SELECT * FROM c WHERE c.state = 'Active' OR NOT IS_DEFINED(c.state)",
            Connection  = "CosmosDBConnection")] IEnumerable<LeaveTypeDto> leaveTypes) =>
        new OkObjectResult(leaveTypes.ToPagedListResponse());

    [Function(nameof(GetLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public IActionResult GetLeaveType(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "leavetypes/{leaveTypeId:guid}")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveTypesContainerName%",
            Connection  = "CosmosDBConnection",
            Id = "{leaveTypeId}",
            PartitionKey = "{leaveTypeId}")] LeaveTypeDto leaveType) =>
        new OkObjectResult(leaveType);

    [Function(nameof(CreateLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public LeaveTypeOutput CreateLeaveType(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "leavetypes")] HttpRequest req,
        [FromBody] LeaveTypeDto leaveType)
    {
        if (leaveType.Properties?.DefaultLimitDays < 0)
        {
            return new()
            {
                Result = new Error($"{nameof(LeaveTypeDto.PropertiesDto.DefaultLimitDays)} cannot be less than 0.", System.Net.HttpStatusCode.BadRequest)
                    .ToObjectResult($"Error occurred while creating a leave type. LeaveTypeId = {leaveType.Id}.")
            };
        }

        return new()
        {
            Result = new CreatedResult($"/leavetypes/{leaveType.Id}", leaveType),
            LeaveType = leaveType
        };
    }

    [Function(nameof(UpdateLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public LeaveTypeOutput UpdateLeaveType(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "leavetypes/{leaveTypeId:guid}")] HttpRequest req,
        Guid leaveTypeId, [FromBody] LeaveTypeDto leaveType)
    {
        if (leaveTypeId != leaveType.Id)
        {
            return new()
            {
                Result = new Error($"{nameof(LeaveTypeDto.Id)} cannot be different than leaveTypeId.", System.Net.HttpStatusCode.BadRequest)
                    .ToObjectResult($"Error occurred while updating a leave type. LeaveTypeId = {leaveType.Id}.")
            };
        }
        if (leaveType.Properties?.DefaultLimitDays < 0)
        {
            return new()
            {
                Result = new Error($"{nameof(LeaveTypeDto.PropertiesDto.DefaultLimitDays)} cannot be less than 0.", System.Net.HttpStatusCode.BadRequest)
                    .ToObjectResult($"Error occurred while updating a leave type. LeaveTypeId = {leaveType.Id}.")
            };
        }

        return new()
        {
            Result = new OkObjectResult(leaveType),
            LeaveType = leaveType
        };
    }

    [Function(nameof(RemoveLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public LeaveTypeOutput RemoveLeaveType(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "leavetypes/{leaveTypeId:guid}")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%LeaveTypesContainerName%",
            Connection  = "CosmosDBConnection",
            Id = "{leaveTypeId}",
            PartitionKey = "{leaveTypeId}")] LeaveTypeDto leaveTypeFromDb)
    {
        var deletedLeaveType = leaveTypeFromDb with { State = LeaveTypeDto.LeaveTypeState.Inactive };
        return new()
        {
            Result = new NoContentResult(),
            LeaveType = deletedLeaveType
        };
    }

    public class LeaveTypeOutput
    {
        [HttpResult]
        public IActionResult Result { get; set; }

        [CosmosDBOutput("%DatabaseName%", "%LeaveTypesContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public LeaveTypeDto? LeaveType { get; set; }
    }
}
