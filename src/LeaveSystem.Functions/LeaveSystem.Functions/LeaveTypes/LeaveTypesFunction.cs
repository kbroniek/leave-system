namespace LeaveSystem.Functions.LeaveTypes;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

public class LeaveTypesFunction
{
    private static GetLeaveTypeDto holidayLeave = new(
        Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
        "urlop wypoczynkowy",
        1,
        new GetLeaveTypeDto.LeaveTypePropertiesDto(
            Color: "#0137C9",
            Catalog: LeaveTypeCatalog.Holiday
        )
    );

    [Function(nameof(GetLeaveTypes))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)}")]
    public async Task<IActionResult> GetLeaveTypes([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leavetypes")] HttpRequest req,
        [CosmosDBInput(
        databaseName: "%DatabaseName%",
        containerName: "%LeaveTypesContainerName%",
        Connection  = "CosmosDBConnection")] IEnumerable<GetLeaveTypeDto> leaveTypes) => new OkObjectResult(leaveTypes.ToPagedListResponse());

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
        PartitionKey = "{leaveTypeId}")] GetLeaveTypeDto leaveType) => new OkObjectResult(leaveType);

    [Function(nameof(CreateLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public IActionResult CreateLeaveType([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leavetypes")] HttpRequest req)
    {
        return new CreatedResult($"/leavetypes/{holidayLeave.Id}", holidayLeave);
    }

    [Function(nameof(UpdateLeaveType))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.LeaveTypeAdmin)}")]
    public IActionResult UpdateLeaveType([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leavetypes/{leaveTypeId:guid}")] HttpRequest req, Guid leaveTypeId)
    {
        return new OkObjectResult(holidayLeave);
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
