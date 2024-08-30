namespace LeaveSystem.Functions.LeaveRequests;

using System.Threading;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Getting;
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
    CreateLeaveRequestService createLeaveRequestService,
    GetLeaveRequestService getLeaveRequestService,
    AcceptLeaveRequestService acceptLeaveRequestService,
    ILogger<LeaveRequestsFunction> logger)
{
    [Function(nameof(SearchLeaveRequests))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> SearchLeaveRequests([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leaverequests")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();
        var queryResult = req.HttpContext.BindSearchLeaveRequests();
        var leaveRequests = new[] {
            new SearchLeaveRequestsResultDto(
                Guid.Parse("55d4c226-206d-4449-bf5d-0c0065b80fff"),
                queryResult.DateFrom ?? DateOnly.FromDateTime(DateTime.UtcNow),
                queryResult.DateTo ?? DateOnly.FromDateTime(DateTime.UtcNow),
                TimeSpan.FromHours(8),
                Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
                LeaveRequestStatus.Accepted,
                userId,
                TimeSpan.FromHours(8)),
            new SearchLeaveRequestsResultDto(
                Guid.Parse("55d4c226-206d-4449-bf5d-0c0065b80ff1"),
                queryResult.DateFrom ?? DateOnly.FromDateTime(DateTime.UtcNow),
                queryResult.DateTo ?? DateOnly.FromDateTime(DateTime.UtcNow),
                TimeSpan.FromHours(8),
                Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
                LeaveRequestStatus.Rejected,
                userId,
                TimeSpan.FromHours(8))
            };

        return new OkObjectResult(leaveRequests.ToPagedListResponse());
    }

    [Function(nameof(GetLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> GetLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leaverequests/{leaveRequestId:guid}")] HttpRequest req, Guid leaveRequestId, CancellationToken cancellationToken)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var result = await getLeaveRequestService.Get(leaveRequestId, cancellationToken);

        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(Map(leaveRequest)),
            error => error.ToObjectResult("Error occurred while getting a leave request details. LeaveRequestId = {leaveRequestDto.LeaveRequestId}."));
    }

    [Function(nameof(CreateLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
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
            leaveRequestDto.WorkingHours,
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
            leaveRequestDto.WorkingHours,
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
        var result = await acceptLeaveRequestService.AcceptAsync(
            leaveRequestId,
            changeStatus.Remark,
            userModel,
            DateTimeOffset.Now,
            cancellationToken
        );
        return result.Match<IActionResult>(
            leaveRequest => new OkObjectResult(Map(leaveRequest)),
            error => error.ToObjectResult("Error occurred while accepting a leave request. LeaveRequestId = {leaveRequestDto.LeaveRequestId}."));
    }

    [Function(nameof(RejectStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> RejectStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/reject")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();

        var now = DateTimeOffset.UtcNow;
        var leaveRequest = new GetLeaveRequestDto(
            leaveRequestId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            LeaveRequestStatus.Rejected,
            userModel,
            userModel,
            userModel,
            TimeSpan.FromHours(8),
            now.AddDays(-1),
            now,
            [
                new GetLeaveRequestDto.RemarksDto("Test remark", userModel, now.AddDays(-1)),
                new GetLeaveRequestDto.RemarksDto(changeStatus.Remark, userModel, now)
            ]);
        return new OkObjectResult(leaveRequest);
    }

    [Function(nameof(CancelStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CancelStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/cancel")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel().MapToLeaveRequestUser();

        var now = DateTimeOffset.UtcNow;
        var leaveRequest = new GetLeaveRequestDto(
            leaveRequestId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            LeaveRequestStatus.Canceled,
            userModel,
            userModel,
            userModel,
            TimeSpan.FromHours(8),
            now.AddDays(-1),
            now,
            [
                new GetLeaveRequestDto.RemarksDto("Test remark", userModel, now.AddDays(-1)),
                new GetLeaveRequestDto.RemarksDto(changeStatus.Remark, userModel, now)
            ]);
        return new OkObjectResult(leaveRequest);
    }

    private GetLeaveRequestDto Map(LeaveRequest leaveRequest) =>
        new GetLeaveRequestDto(
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
