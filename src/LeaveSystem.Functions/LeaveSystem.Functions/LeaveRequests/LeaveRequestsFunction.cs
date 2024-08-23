namespace LeaveSystem.Functions.LeaveRequests;

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
        Route = "leaverequests/{leaveRequestId:guid}")] HttpRequest req, Guid leaveRequestId)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var leaveRequest = await getLeaveRequestService.Get(leaveRequestId);

        var leaveRequestDto = new GetLeaveRequestDto(
            leaveRequestId,
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
        return new OkObjectResult(leaveRequestDto);
    }

    [Function(nameof(CreateLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CreateLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequests")] HttpRequest req, [FromBody] CreateLeaveRequestDto leaveRequest)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();
        var userModel = req.HttpContext.User.CreateModel();
        await createLeaveRequestService.Create(LeaveRequestCreated.Create(
            leaveRequest.LeaveRequestId,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.WorkingHours,
            leaveRequest.LeaveTypeId,
            leaveRequest.Remark,
            userModel,
            userModel,
            leaveRequest.WorkingHours,
            DateTimeOffset.UtcNow
            ));

        var now = DateTimeOffset.UtcNow;
        var leaveRequestCreated = new GetLeaveRequestDto(
            leaveRequest.LeaveRequestId,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.WorkingHours,
            leaveRequest.LeaveTypeId,
            LeaveRequestStatus.Pending,
            userModel,
            userModel,
            userModel,
            leaveRequest.WorkingHours,
            now,
            now,
            new[]
            {
                new GetLeaveRequestDto.RemarksDto(leaveRequest.Remark, userModel, now)
            });
        return new CreatedResult($"leaverequest/{leaveRequest.LeaveRequestId}", leaveRequestCreated);
    }

    [Function(nameof(CreateLeaveRequestOnBehalf))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CreateLeaveRequestOnBehalf([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequests/onbehalf")] HttpRequest req, [FromBody] CreateLeaveRequestOnBehalfDto leaveRequest)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var user = req.HttpContext.User.CreateModel();

        var now = DateTimeOffset.UtcNow;
        var leaveRequestCreated = new GetLeaveRequestDto(
            leaveRequest.LeaveRequestId,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.WorkingHours,
            leaveRequest.LeaveTypeId,
            LeaveRequestStatus.Pending,
            user, //leaveRequest.OwnerUserId,
            user,
            user,
            leaveRequest.WorkingHours,
            now,
            now,
            [
                new GetLeaveRequestDto.RemarksDto(leaveRequest.Remark, user, now)
            ]);
        return new CreatedResult($"leaverequest/{leaveRequest.LeaveRequestId}", leaveRequestCreated);
    }

    [Function(nameof(AcceptStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> AcceptStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/accept")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel();
        await acceptLeaveRequestService.Create(LeaveRequestAccepted.Create(
            leaveRequestId,
            changeStatus.Remark,
            userModel,
            DateTimeOffset.UtcNow
        ));

        var now = DateTimeOffset.UtcNow;
        var leaveRequest = new GetLeaveRequestDto(
            leaveRequestId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            LeaveRequestStatus.Accepted,
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

    [Function(nameof(RejectStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> RejectStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequests/{leaveRequestId:guid}/reject")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userModel = req.HttpContext.User.CreateModel();

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

        var userModel = req.HttpContext.User.CreateModel();

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
}
