using System.Security.Claims;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions.LeaveRequests
{
    public class LeaveRequestsFunction
    {
        private readonly ILogger<LeaveRequestsFunction> _logger;

        public LeaveRequestsFunction(ILogger<LeaveRequestsFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SearchLeaveRequests))]
        [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.HumanResource)},{nameof(RoleType.DecisionMaker)}")]
        public async Task<IActionResult> SearchLeaveRequests([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var userId = req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var queryResult = SearchLeaveRequestsQuery.Bind(req.HttpContext);
            var leaveRequests = new PagedListResponse<SearchLeaveRequestDto>(Enumerable.Range(1, 1)
                .Select(x => new SearchLeaveRequestDto(
                    Guid.Parse("55d4c226-206d-4449-bf5d-0c0065b80fff"),
                    queryResult.DateFrom ?? DateTimeOffset.UtcNow,
                    queryResult.DateTo ?? DateTimeOffset.UtcNow,
                    TimeSpan.FromHours(8),
                    Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
                    LeaveSystem.Shared.LeaveRequests.LeaveRequestStatus.Accepted,
                    userId,
                    TimeSpan.FromHours(8))), 1, false);

            return new OkObjectResult(leaveRequests);
        }
    }
}
