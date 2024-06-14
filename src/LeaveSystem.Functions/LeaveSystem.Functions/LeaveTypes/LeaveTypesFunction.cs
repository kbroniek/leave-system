using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Functions.LeaveTypes
{
    public class LeaveTypesFunction
    {
        private static GetLeaveTypeDto holidayLeave = new(
            Guid.Parse("ae752d4b-0368-4d46-8efa-9ef2ee248fa9"),
            "urlop wypoczynkowy",
            1,
            new GetLeaveTypeDto.LeaveTypeDtoProperties(
                DefaultLimitDays: 26,
                IncludeFreeDays: false,
                Color: "#0137C9",
                Catalog: LeaveTypeCatalog.Holiday
            )
        );

        private static GetLeaveTypeDto onDemandLeave = new(
            Id: Guid.Parse("6e1a75ca-07dc-45aa-9451-57e1cefd7ee6"),
            Name: "urlop na żądanie",
            BaseLeaveTypeId: holidayLeave.Id,
            Order: 2,
            Properties: new GetLeaveTypeDto.LeaveTypeDtoProperties(
                DefaultLimitDays: 4,
                IncludeFreeDays: false,
                Color: "#B88E1E",
                Catalog: LeaveTypeCatalog.OnDemand
            )
        );

        private static GetLeaveTypeDto sickLeave = new(
            Id: Guid.Parse("c91558cc-5d58-4a77-b46e-e6a69ac14cf4"),
            Name: "niezdolność do pracy z powodu choroby",
            Order: 3,
            Properties: new GetLeaveTypeDto.LeaveTypeDtoProperties(
                IncludeFreeDays: true,
                Color: "#FF3333",
                Catalog: LeaveTypeCatalog.Sick
            )
        );

        private static GetLeaveTypeDto saturdayLeave = new(
            Id: Guid.Parse("2dfb7d85-70d8-4acc-a565-62fde19b7cd1"),
            Name: "urlop za sobotę",
            Order: 14,
            Properties: new GetLeaveTypeDto.LeaveTypeDtoProperties(
                DefaultLimitDays: 1,
                IncludeFreeDays: false,
                Color: "#FFFF33",
                Catalog: LeaveTypeCatalog.Saturday
            )
        );
        private readonly ILogger logger;

        public LeaveTypesFunction(ILoggerFactory loggerFactory) => logger = loggerFactory.CreateLogger<LeaveTypesFunction>();

        [Function(nameof(GetLeaveTypes))]
        [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)}")]
        public async Task<IActionResult> GetLeaveTypes([HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "leavetype")] HttpRequest req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var leaveTypes = new GetLeaveTypeDto[]
            {
                holidayLeave,
                onDemandLeave,
                sickLeave,
                saturdayLeave
            };

            return new OkObjectResult(leaveTypes);
        }

        [Function(nameof(GetLeaveType))]
        [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)}")]
        public async Task<IActionResult> GetLeaveType([HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "leavetype/{leaveTypeId:guid}")] HttpRequest req, Guid leaveTypeId)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            return new OkObjectResult(holidayLeave);
        }
    }
}
