namespace LeaveSystem.Functions.LeaveRequests;

using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using LeaveSystem.Domain;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class LeaveRequestsFunction(
    SearchLeaveRequestService searchLeaveRequestService,
    CreateLeaveRequestService createLeaveRequestService,
    GetLeaveRequestService getLeaveRequestService,
    AcceptLeaveRequestService acceptLeaveRequestService,
    RejectLeaveRequestService rejectLeaveRequestService,
    CancelLeaveRequestService cancelLeaveRequestService,
    EmployeeService employeeService)
{
    [Function(nameof(SearchLeaveRequests))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.HumanResource)}")]
    public async Task<IActionResult> SearchLeaveRequests([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leaverequests")] HttpRequest req, CancellationToken cancellationToken)
    {
        var queryResult = req.HttpContext.BindSearchLeaveRequests();
        // If user has greater privilege then can read all data. Otherwise can read only his leave requests.
        var isGlobalAdmin = req.HttpContext.User.IsInRole(nameof(RoleType.GlobalAdmin));
        var isDecisionMaker = req.HttpContext.User.IsInRole(nameof(RoleType.DecisionMaker));
        var isHumanResource = req.HttpContext.User.IsInRole(nameof(RoleType.HumanResource));
        var limitToUserIds = isGlobalAdmin || isDecisionMaker || isHumanResource ? queryResult.AssignedToUserIds : [req.HttpContext.GetUserId()];

        var result = await searchLeaveRequestService.Search(
            queryResult.ContinuationToken, queryResult.DateFrom, queryResult.DateTo,
            queryResult.LeaveTypeIds, queryResult.Statuses, limitToUserIds, cancellationToken);

        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(new
            {
                Items = result.Value.results,
                result.Value.search.ContinuationToken,
                Search = result.Value.search
            }),
            error => error.ToObjectResult($"Error occurred while searching leave requests. Query = {JsonSerializer.Serialize(queryResult)}."));
    }

    [Function(nameof(GetLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.HumanResource)}")]
    public async Task<IActionResult> GetLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leaverequests/{leaveRequestId:guid}")] HttpRequest req, Guid leaveRequestId, CancellationToken cancellationToken)
    {
        var result = await getLeaveRequestService.Get(leaveRequestId, cancellationToken);
        var resultPermission = IfDifferentEmployeThenReturnError(req.HttpContext.User, result);
        if (resultPermission.IsFailure)
        {
            return resultPermission.Error.ToObjectResult($"Error occurred while getting a leave request details. LeaveRequestId = {leaveRequestId}.");
        }

        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(Map(leaveRequest)),
            error => error.ToObjectResult($"Error occurred while getting a leave request details. LeaveRequestId = {leaveRequestId}."));
    }

    [Function(nameof(CreateLeaveRequest))]
    [Authorize(Roles = nameof(RoleType.Employee))]
    public async Task<IActionResult> CreateLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequests")] HttpRequest req, [FromBody] CreateLeaveRequestDto leaveRequestDto, CancellationToken cancellationToken)
    {
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
            error => error.ToObjectResult($"Error occurred while creating a leave request. LeaveRequestId = {leaveRequestDto.LeaveRequestId}."));
    }

    [Function(nameof(CreateLeaveRequestOnBehalf))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CreateLeaveRequestOnBehalf([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequests/onbehalf")] HttpRequest req, [FromBody] CreateLeaveRequestOnBehalfDto leaveRequestDto, CancellationToken cancellationToken)
    {
        var employeeResult = await employeeService.Get(leaveRequestDto.AssignedToId, cancellationToken);
        if (employeeResult.IsFailure)
        {
            return employeeResult.Error.ToObjectResult($"An error occurred while preparing to create a leave request on behalf of another user. LeaveRequestId = {leaveRequestDto.LeaveRequestId}.");
        }
        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();
        var result = await createLeaveRequestService.CreateAsync(
            leaveRequestDto.LeaveRequestId,
            leaveRequestDto.DateFrom,
            leaveRequestDto.DateTo,
            leaveRequestDto.Duration,
            leaveRequestDto.LeaveTypeId,
            leaveRequestDto.Remark,
            userModel,
            new LeaveRequestUserDto(employeeResult.Value.Id, employeeResult.Value.Name),
            leaveRequestDto.WorkingHours,
            DateTimeOffset.Now,
            cancellationToken);
        return result.Match<IActionResult>(
            leaveRequest => new CreatedResult($"leaverequest/{leaveRequestDto.LeaveRequestId}", Map(leaveRequest)),
            error => error.ToObjectResult($"An error occurred while creating a leave request on behalf of another user. LeaveRequestId = {leaveRequestDto.LeaveRequestId}."));
    }

    [Function(nameof(AcceptStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> AcceptStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/accept")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus, CancellationToken cancellationToken)
    {
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
    [Authorize(Roles = $"{nameof(RoleType.Employee)}")]
    public async Task<IActionResult> CancelStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/cancel")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus, CancellationToken cancellationToken)
    {
        // TODO: Optimize performance
        var resultGet = await getLeaveRequestService.Get(leaveRequestId, cancellationToken);
        var resultPermission = IfDifferentEmployeThenReturnError(req.HttpContext.User, resultGet);
        if (resultPermission.IsFailure)
        {
            return resultPermission.Error.ToObjectResult($"Error occurred while getting a leave request details. LeaveRequestId = {leaveRequestId}.");
        }
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

    private static Result<Error> IfDifferentEmployeThenReturnError(ClaimsPrincipal user, Result<LeaveRequest, Error> result)
    {
        if (result.IsSuccess && !user.IsInRole(nameof(RoleType.DecisionMaker)) && !user.IsInRole(nameof(RoleType.GlobalAdmin)))
        {
            var userId = user.GetUserId();
            //Employee can only see his own leave request.
            if (userId != result.Value.AssignedTo.Id)
            {
                return new Error("Permission denied.", System.Net.HttpStatusCode.Forbidden, ErrorCodes.FORBIDDEN_OPERATION);
            }
        }
        return Result.Default;
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
