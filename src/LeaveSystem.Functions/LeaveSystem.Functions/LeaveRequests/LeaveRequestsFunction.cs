namespace LeaveSystem.Functions.LeaveRequests;

using System.Threading;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Canceling;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Getting;
using LeaveSystem.Domain.LeaveRequests.Rejecting;
using LeaveSystem.Domain.LeaveRequests.Searching;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class LeaveRequestsFunction(
    SearchLeaveRequestService searchLeaveRequestService,
    CreateLeaveRequestService createLeaveRequestService,
    GetLeaveRequestService getLeaveRequestService,
    AcceptLeaveRequestService acceptLeaveRequestService,
    RejectLeaveRequestService rejectLeaveRequestService,
    CancelLeaveRequestService cancelLeaveRequestService,
    ILogger<LeaveRequestsFunction> logger)
{
    [Function(nameof(SearchLeaveRequests))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> SearchLeaveRequests([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leaverequests")] HttpRequest req, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var queryResult = req.HttpContext.BindSearchLeaveRequests();
        // If user has greater privilege then can read all data. Otherwise can read only his leave requests.
        var isGlobalAdmin = req.HttpContext.User.IsInRole(nameof(RoleType.GlobalAdmin));
        var isDecisionMaker = req.HttpContext.User.IsInRole(nameof(RoleType.DecisionMaker));
        var limitToUserIds = isGlobalAdmin || isDecisionMaker ? queryResult.AssignedToUserIds : [req.HttpContext.GetUserId()];

        (var leaveRequests, var search) = await searchLeaveRequestService.Search(
            queryResult.ContinuationToken, queryResult.DateFrom, queryResult.DateTo,
            queryResult.LeaveTypeIds, queryResult.Statuses, limitToUserIds, cancellationToken);

        return new OkObjectResult(new
        {
            Items = leaveRequests,
            Search = search
        });
    }

    [Function(nameof(GetLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> GetLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leaverequests/{leaveRequestId:guid}")] HttpRequest req, Guid leaveRequestId, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        //TODO: Security. Only special roles can read all leave requests
        var result = await getLeaveRequestService.Get(leaveRequestId, cancellationToken);

        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(Map(leaveRequest)),
            error => error.ToObjectResult("Error occurred while getting a leave request details. LeaveRequestId = {leaveRequestDto.LeaveRequestId}."));
    }

    [Function(nameof(CreateLeaveRequest))]
    [Authorize(Roles = nameof(RoleType.Employee))]
    public async Task<IActionResult> CreateLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequests")] HttpRequest req, [FromBody] CreateLeaveRequestDto leaveRequestDto, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();
        var result = await createLeaveRequestService.CreateAsync(
            leaveRequestDto.LeaveRequestId,
            leaveRequestDto.DateFrom,
            leaveRequestDto.DateTo,
            leaveRequestDto.Duration,
            leaveRequestDto.LeaveTypeId,
            leaveRequestDto.Remark,
            userModel,
            userModel,
            leaveRequestDto.WorkingHours,
            DateTimeOffset.Now,
            cancellationToken);
        return result.Match<IActionResult>(
            leaveRequest => new CreatedResult($"leaverequest/{leaveRequestDto.LeaveRequestId}", Map(leaveRequest)),
            error => error.ToObjectResult("Error occurred while creating a leave request. LeaveRequestId = {leaveRequestDto.LeaveRequestId}."));
    }

    [Function(nameof(CreateLeaveRequestOnBehalf))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CreateLeaveRequestOnBehalf([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequests/onbehalf")] HttpRequest req, [FromBody] CreateLeaveRequestOnBehalfDto leaveRequestDto, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();
        var result = await createLeaveRequestService.CreateAsync(
            leaveRequestDto.LeaveRequestId,
            leaveRequestDto.DateFrom,
            leaveRequestDto.DateTo,
            leaveRequestDto.Duration,
            leaveRequestDto.LeaveTypeId,
            leaveRequestDto.Remark,
            userModel,
            //TODO Check if user exists and active in graph API
            new LeaveRequestUserDto(leaveRequestDto.AssignedToId, null),
            leaveRequestDto.WorkingHours,
            DateTimeOffset.Now,
            cancellationToken);
        return result.Match<IActionResult>(
            leaveRequest => new CreatedResult($"leaverequest/{leaveRequestDto.LeaveRequestId}", Map(leaveRequest)),
            error => error.ToObjectResult($"Error occurred while creating a leave request on behalf of another user. LeaveRequestId = {leaveRequestDto.LeaveRequestId}."));
    }

    [Function(nameof(AcceptStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> AcceptStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/accept")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();
        var result = await acceptLeaveRequestService.Accept(
            leaveRequestId,
            changeStatus.Remark,
            userModel,
            DateTimeOffset.Now,
            cancellationToken
        );
        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(Map(leaveRequest)),
            error => error.ToObjectResult($"Error occurred while accepting a leave request. LeaveRequestId = {leaveRequestId}."));
    }

    [Function(nameof(RejectStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> RejectStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/reject")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();
        var result = await rejectLeaveRequestService.Reject(
            leaveRequestId,
            changeStatus.Remark,
            userModel,
            DateTimeOffset.Now,
            cancellationToken
        );
        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(Map(leaveRequest)),
            error => error.ToObjectResult($"Error occurred while rejecting a leave request. LeaveRequestId = {leaveRequestId}."));
    }

    [Function(nameof(CancelStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CancelStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/cancel")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();
        var result = await cancelLeaveRequestService.Cancel(
            leaveRequestId,
            changeStatus.Remark,
            userModel,
            DateTimeOffset.Now,
            cancellationToken
        );
        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(Map(leaveRequest)),
            error => error.ToObjectResult($"Error occurred while canceling a leave request. LeaveRequestId = {leaveRequestId}."));
    }

    private GetLeaveRequestDto Map(LeaveRequest leaveRequest) =>
        new(
            leaveRequest.Id,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.Duration,
            leaveRequest.LeaveTypeId,
            leaveRequest.Status,
            leaveRequest.AssignedTo,
            leaveRequest.LastModifiedBy,
            leaveRequest.CreatedBy,
            leaveRequest.WorkingHours,
            leaveRequest.CreatedDate,
            leaveRequest.LastModifiedDate,
            leaveRequest.Remarks.Select(r => new GetLeaveRequestDto.RemarksDto(r.Remarks, r.CreatedBy, r.CreatedDate)));
}
