namespace LeaveSystem.Functions.LeaveRequests;
using System.Security.Claims;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class LeaveRequestsFunction
{
    private readonly ILogger<LeaveRequestsFunction> logger;

    public LeaveRequestsFunction(ILogger<LeaveRequestsFunction> logger)
    {
        this.logger = logger;
    }

    [Function(nameof(SearchLeaveRequests))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.HumanResource)},{nameof(RoleType.DecisionMaker)}")]
    public async Task<IActionResult> SearchLeaveRequests([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
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

        return new OkObjectResult(leaveRequests.CreatePagedListResponse());
    }
}
