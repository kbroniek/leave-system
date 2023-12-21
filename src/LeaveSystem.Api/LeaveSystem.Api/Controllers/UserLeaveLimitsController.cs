using Ardalis.GuardClauses;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Periods;
using LeaveSystem.Shared;
using LeaveSystem.Shared.UserLeaveLimits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace LeaveSystem.Api.Controllers;

//TODO: Set permissions to get limits only for current user.
[Route("api/[controller]")]
[Authorize]
public class UserLeaveLimitsController : ODataController
{
    private readonly LeaveSystemDbContext dbContext;
    private readonly GenericCrudService<UserLeaveLimit, Guid> crudService;
    private const string InvalidModelMessage = "This request is not valid oDataRequest";

    public UserLeaveLimitsController(LeaveSystemDbContext dbContext,
        GenericCrudService<UserLeaveLimit, Guid> crudService)
    {
        this.dbContext = dbContext;
        this.crudService = crudService;
    }

    [HttpGet]
    [EnableQuery]
    public IQueryable<UserLeaveLimit>? Get()
    {
        return crudService.Get();
    }

    [HttpGet("{key}")]
    [EnableQuery]
    public virtual SingleResult<UserLeaveLimit> Get([FromODataUri] Guid key)
    {
        var entity = crudService.GetSingleAsQueryable(key);
        return SingleResult.Create(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AddUserLeaveLimitDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new BadHttpRequestException(InvalidModelMessage);
        }
        var entity = new UserLeaveLimit()
        {
            Id = new Guid(),
            OverdueLimit = dto.OverdueLimit,
            Limit = dto.Limit,
            AssignedToUserId = dto.AssignedToUserId,
            ValidSince = dto.ValidSince,
            ValidUntil = dto.ValidUntil,
            LeaveTypeId = dto.LeaveTypeId,
            Property = new()
            {
                Description = dto.Property?.Description
            }
        };
        var addedEntity = await crudService.AddAsync(entity, cancellationToken);
        await SetStartAndEndDateForNeighbourLimitsIfDontHaveAsync(addedEntity, cancellationToken);
        return Created(addedEntity);
    }

    private async Task SetStartAndEndDateForNeighbourLimitsIfDontHaveAsync(
        UserLeaveLimit limit,
        CancellationToken cancellationToken)
    {
        var userLimitsForSameLeaveType = dbContext.UserLeaveLimits
            .Where(ull => ull.LeaveTypeId == limit.LeaveTypeId && ull.AssignedToUserId == limit.AssignedToUserId);;
        if (limit.ValidSince.HasValue)
        {
            await SetEndDateForPreviousLimitIfDontHaveAsync(userLimitsForSameLeaveType, limit.ValidSince.Value, cancellationToken);
        }
        if (limit.ValidUntil.HasValue)
        {
            await SetStartDateForNextLimitIfDontHaveAsync(userLimitsForSameLeaveType, limit.ValidUntil.Value, cancellationToken);
        }
    }
    
    private async Task SetEndDateForPreviousLimitIfDontHaveAsync(
        IQueryable<UserLeaveLimit> userLeaveLimits,
        DateTimeOffset validSince,
        CancellationToken cancellationToken)
    {
        var limitWithoutEndDateBeforeThisLimit = await userLeaveLimits
            .OrderBy(x => x.ValidSince)
            .LastOrDefaultAsync(
                x => !x.ValidUntil.HasValue && x.ValidSince < validSince
                , cancellationToken);
        if (limitWithoutEndDateBeforeThisLimit is null)
        {
            return;
        }
        limitWithoutEndDateBeforeThisLimit.ValidUntil = validSince;
        dbContext.Update(limitWithoutEndDateBeforeThisLimit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SetStartDateForNextLimitIfDontHaveAsync(
        IQueryable<UserLeaveLimit> userLeaveLimits,
        DateTimeOffset validUntil,
        CancellationToken cancellationToken)
    {
        var limitWithoutStartDateAfterThisLimit = await userLeaveLimits
            .OrderBy(x => x.ValidSince)
            .FirstOrDefaultAsync(
                x => !x.ValidSince.HasValue && x.ValidUntil > validUntil
                , cancellationToken);
        if (limitWithoutStartDateAfterThisLimit is null)
        {
            return;
        }
        limitWithoutStartDateAfterThisLimit.ValidSince = validUntil;
        dbContext.Update(limitWithoutStartDateAfterThisLimit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch(
        [FromODataUri] Guid key,
        [FromBody] Delta<UserLeaveLimit> delta,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new BadHttpRequestException(InvalidModelMessage);
        }
        var updatedEntity = await crudService.PatchAsync(key, delta, cancellationToken);
        await SetStartAndEndDateForNeighbourLimitsIfDontHaveAsync(updatedEntity, cancellationToken);
        return Updated(updatedEntity);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromODataUri] Guid key, CancellationToken cancellationToken = default)
    {
        await crudService.DeleteAsync(key, cancellationToken);
        return NoContent();
    }
}