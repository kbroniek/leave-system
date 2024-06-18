using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions.LeaveTypes
{
    public class GetLeaveStatusSettingsFunction
    {
        private readonly ILogger logger;

        public GetLeaveStatusSettingsFunction(ILoggerFactory loggerFactory) => logger = loggerFactory.CreateLogger<GetLeaveStatusSettingsFunction>();

        [Function(nameof(GetLeaveStatusSettings))]
        [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.HumanResource)}")]
        public async Task<IActionResult> GetLeaveStatusSettings([HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "settings/leavestatus")] HttpRequest req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var leaveTypes = new[]
            {
                new GetLeaveStatusSettingsDto(LeaveRequestStatus.Pending, "#FF00FF"),
                new GetLeaveStatusSettingsDto(LeaveRequestStatus.Canceled, "#FF0000"),
                new GetLeaveStatusSettingsDto(LeaveRequestStatus.Accepted, "#FF11FF"),
                new GetLeaveStatusSettingsDto(LeaveRequestStatus.Deprecated, "#F1122"),
                new GetLeaveStatusSettingsDto(LeaveRequestStatus.Rejected, "#1122FF")
            };

            return new OkObjectResult(leaveTypes.ToPagedListResponse());
        }
    }
}
