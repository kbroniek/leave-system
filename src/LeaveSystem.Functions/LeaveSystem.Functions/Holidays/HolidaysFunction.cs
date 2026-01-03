namespace LeaveSystem.Functions.Holidays;

using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public record GetHolidaysQuery(DateOnly DateFrom, DateOnly DateTo);

public class HolidaysFunction(HolidaysService holidaysService, ILogger<HolidaysFunction> logger)
{
    [Function(nameof(GetHolidays))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.Employee)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.HumanResource)}")]
    public IActionResult GetHolidays(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "settings/holidays")] HttpRequest req
        )
    {
        var query = req.HttpContext.BindGetHolidaysQuery();
        if (query.IsFailure)
        {
            return query.Error.ToObjectResult("Error occurred while getting holidays.");
        }
        if (query.Value.DateFrom > query.Value.DateTo)
        {
            return Error.BadRequest($"{nameof(query.Value.DateFrom)} cannot be greater than {nameof(query.Value.DateTo)}").ToObjectResult("Error occurred while getting holidays.");
        }
        var result = holidaysService.GetHolidays(query.Value.DateFrom, query.Value.DateTo);
        return new OkObjectResult(result.ToPagedListResponse());
    }
}
