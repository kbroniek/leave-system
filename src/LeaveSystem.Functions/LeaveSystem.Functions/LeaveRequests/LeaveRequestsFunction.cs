namespace LeaveSystem.Functions.LeaveRequests;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class LeaveRequestsFunction
{
    private readonly ILogger<LeaveRequestsFunction> logger;

    public LeaveRequestsFunction(ILogger<LeaveRequestsFunction> logger)
    {
        this.logger = logger;
    }

    [Function(nameof(SearchLeaveRequests))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> SearchLeaveRequests([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "leaverequest")] HttpRequest req)
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
                LeaveSystem.Shared.LeaveRequests.LeaveRequestStatus.Accepted,
                userId,
                TimeSpan.FromHours(8)),
            new SearchLeaveRequestsResultDto(
                Guid.Parse("55d4c226-206d-4449-bf5d-0c0065b80ff1"),
                queryResult.DateFrom ?? DateOnly.FromDateTime(DateTime.UtcNow),
                queryResult.DateTo ?? DateOnly.FromDateTime(DateTime.UtcNow),
                TimeSpan.FromHours(8),
                Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
                LeaveSystem.Shared.LeaveRequests.LeaveRequestStatus.Rejected,
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
        Route = "leaverequest/{leaveRequestId:guid}")] HttpRequest req, Guid leaveRequestId)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();

        var leaveRequest = new GetLeaveRequestDto(
            leaveRequestId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            LeaveSystem.Shared.LeaveRequests.LeaveRequestStatus.Accepted,
            userId,
            userId,
            userId,
            TimeSpan.FromHours(8),
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            new[]
            {
                new GetLeaveRequestDto.RemarksDto("Test remark", userId, DateTimeOffset.UtcNow.AddDays(-1))
            });
        return new OkObjectResult(leaveRequest);
    }

    [Function(nameof(CreateLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.Employee)}")]
    public async Task<IActionResult> CreateLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequest")] HttpRequest req, [FromBody] CreateLeaveRequestDto leaveRequest)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();

        var now = DateTimeOffset.UtcNow;
        var leaveRequestCreated = new GetLeaveRequestDto(
            leaveRequest.LeaveRequestId,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.WorkingHours,
            leaveRequest.LeaveTypeId,
            LeaveSystem.Shared.LeaveRequests.LeaveRequestStatus.Pending,
            userId,
            userId,
            userId,
            leaveRequest.WorkingHours,
            now,
            now,
            new[]
            {
                new GetLeaveRequestDto.RemarksDto(leaveRequest.Remark, userId, now)
            });
        return new CreatedResult($"leaverequest/{leaveRequest.LeaveRequestId}", leaveRequestCreated);
    }

    [Function(nameof(CreateLeaveRequestOnBehalf))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CreateLeaveRequestOnBehalf([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "post",
        Route = "leaverequest/onbehalf")] HttpRequest req, [FromBody] CreateLeaveRequestOnBehalfDto leaveRequest)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();

        var now = DateTimeOffset.UtcNow;
        var leaveRequestCreated = new GetLeaveRequestDto(
            leaveRequest.LeaveRequestId,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.WorkingHours,
            leaveRequest.LeaveTypeId,
            LeaveRequestStatus.Pending,
            leaveRequest.OwnerUserId,
            userId,
            userId,
            leaveRequest.WorkingHours,
            now,
            now,
            new[]
            {
                new GetLeaveRequestDto.RemarksDto(leaveRequest.Remark, userId, now)
            });
        return new CreatedResult($"leaverequest/{leaveRequest.LeaveRequestId}", leaveRequestCreated);
    }

    [Function(nameof(AcceptStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> AcceptStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequest/{leaveRequestId:guid}/accept")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();

        var now = DateTimeOffset.UtcNow;
        var leaveRequest = new GetLeaveRequestDto(
            leaveRequestId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            LeaveRequestStatus.Accepted,
            userId,
            userId,
            userId,
            TimeSpan.FromHours(8),
            now.AddDays(-1),
            now,
            new[]
            {
                new GetLeaveRequestDto.RemarksDto("Test remark", userId, now.AddDays(-1)),
                new GetLeaveRequestDto.RemarksDto(changeStatus.Remark, userId, now)
            });
        return new OkObjectResult(leaveRequest);
    }

    [Function(nameof(RejectStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> RejectStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequest/{leaveRequestId:guid}/reject")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();

        var now = DateTimeOffset.UtcNow;
        var leaveRequest = new GetLeaveRequestDto(
            leaveRequestId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            LeaveRequestStatus.Rejected,
            userId,
            userId,
            userId,
            TimeSpan.FromHours(8),
            now.AddDays(-1),
            now,
            new[]
            {
                new GetLeaveRequestDto.RemarksDto("Test remark", userId, now.AddDays(-1)),
                new GetLeaveRequestDto.RemarksDto(changeStatus.Remark, userId, now)
            });
        return new OkObjectResult(leaveRequest);
    }

    [Function(nameof(CancelStatusLeaveRequest))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> CancelStatusLeaveRequest([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "put",
        Route = "leaverequest/{leaveRequestId:guid}/cancel")] HttpRequest req, Guid leaveRequestId, [FromBody] ChangeStatusLeaveRequestDto changeStatus)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var userId = req.HttpContext.GetUserId();

        var now = DateTimeOffset.UtcNow;
        var leaveRequest = new GetLeaveRequestDto(
            leaveRequestId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow),
            TimeSpan.FromHours(8),
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            LeaveRequestStatus.Canceled,
            userId,
            userId,
            userId,
            TimeSpan.FromHours(8),
            now.AddDays(-1),
            now,
            new[]
            {
                new GetLeaveRequestDto.RemarksDto("Test remark", userId, now.AddDays(-1)),
                new GetLeaveRequestDto.RemarksDto(changeStatus.Remark, userId, now)
            });
        return new OkObjectResult(leaveRequest);
    }
}
