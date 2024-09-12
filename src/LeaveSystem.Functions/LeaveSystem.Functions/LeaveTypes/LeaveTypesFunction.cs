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
    public async Task<IActionResult> GetLeaveTypes([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leavetypes")] HttpRequest req,
        [CosmosDBInput(
        databaseName: "%DatabaseName%",
        containerName: "%LeaveTypesContainerName%",
        Connection  = "CosmosDBConnection")] IEnumerable<LeaveTypeDto> leaveTypes) => new OkObjectResult(leaveTypes.ToPagedListResponse());

    [Function(nameof(GetLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> GetLeaveType([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leavetypes/{leaveTypeId:guid}")] HttpRequest req,
        [CosmosDBInput(
        databaseName: "%DatabaseName%",
        containerName: "%LeaveTypesContainerName%",
        Connection  = "CosmosDBConnection",
        Id = "{leaveTypeId}",
        PartitionKey = "{leaveTypeId}")] LeaveTypeDto leaveType) => new OkObjectResult(leaveType);

    [Function(nameof(CreateLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public LeaveTypeOutputType CreateLeaveType([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leavetypes")] HttpRequest req, [FromBody] LeaveTypeDto leaveType)
    {
        if (leaveType.Properties?.DefaultLimitDays < 0)
        {
            return new()
            {
                Result = new Error($"{nameof(LeaveTypeDto.PropertiesDto.DefaultLimitDays)} cannot be less than 0.", System.Net.HttpStatusCode.BadRequest)
                    .ToObjectResult($"Error occurred while creating a leave type. LeaveRequestId = {leaveType.Id}.")
            };
        }

        return new()
        {
            Result = new CreatedResult($"/leavetypes/{leaveType.Id}", leaveType),
            LeaveType = leaveType
        };
    }

    public class LeaveTypeOutputType
    {
        [HttpResult]
        public IActionResult Result { get; set; }

        [CosmosDBOutput("%DatabaseName%", "%LeaveTypesContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public LeaveTypeDto? LeaveType { get; set; }
    }

    [Function(nameof(UpdateLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public IActionResult UpdateLeaveType([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leavetypes/{leaveTypeId:guid}")] HttpRequest req, Guid leaveTypeId)
    {
        return new OkObjectResult("");
    }

    [Function(nameof(RemoveLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public IActionResult RemoveLeaveType([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "delete",
        Route = "leavetypes/{leaveTypeId:guid}")] HttpRequest req, Guid leaveTypeId)
    {
        return new NoContentResult();
    }
}
