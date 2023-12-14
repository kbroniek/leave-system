using FluentValidation;
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
    private readonly IValidator<UserLeaveLimit> limitValidator;
    private const string InvalidModelMessage = "This request is not valid oDataRequest";

    public UserLeaveLimitsController(LeaveSystemDbContext dbContext,
        GenericCrudService<UserLeaveLimit, Guid> crudService,
        IValidator<UserLeaveLimit> limitValidator)
    {
        this.dbContext = dbContext;
        this.crudService = crudService;
        this.limitValidator = limitValidator;
    }

    [HttpGet]
    [EnableQuery]
    public IQueryable<UserLeaveLimit>? Get()
    {
        return crudService.Get();
    }

    [HttpGet("{key}")]
    [EnableQuery]
    public SingleResult<UserLeaveLimit> Get([FromODataUri] Guid key)
    {
        var entity = crudService.GetAsQueryable(key);
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

        var leaveTypeExists = await dbContext.LeaveTypes.AnyAsync(lt => lt.Id == dto.LeaveTypeId, cancellationToken);
        if (!leaveTypeExists)
        {
            throw new ValidationException("Leave type with provided Id not exists");
        }

        var overlapsOtherLimit = await CheckIfPeriodOverlapsAnyLimit(dto.ValidSince, dto.ValidUntil, cancellationToken);
        if (overlapsOtherLimit)
        {
            throw new ValidationException(
                "Cannot create a new limit in this time. The other limit is overlapping with this date.");
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
        await limitValidator.ValidateAsync(entity, cancellationToken);
        var addedEntity = await crudService.AddAsync(entity, cancellationToken);
        if (addedEntity.ValidSince.HasValue)
        {
            await SetEndDateForPreviousLimitIfDontHaveAsync(addedEntity.ValidSince.Value, cancellationToken);
        }
        if (addedEntity.ValidUntil.HasValue)
        {
            await SetStartDateForNextLimitIfDontHaveAsync(addedEntity.ValidUntil.Value, cancellationToken);
        }

        return Created(addedEntity);
    }

    private Task<bool> CheckIfPeriodOverlapsAnyLimit(DateTimeOffset? dateFrom, DateTimeOffset? dateTo,
        CancellationToken cancellationToken)
    {
        return dbContext.UserLeaveLimits.AnyAsync(
            ull =>
                !(
                    // checking if periods can't overlap
                    (!ull.ValidSince.HasValue && !ull.ValidUntil.HasValue) ||
                    (!dateFrom.HasValue && !dateTo.HasValue) ||
                    (!ull.ValidSince.HasValue && !dateFrom.HasValue) ||
                    (!ull.ValidUntil.HasValue && !dateTo.HasValue)
                ) && (
                    // checking if periods overlaps
                    (!ull.ValidSince.HasValue && ull.ValidUntil >= dateFrom && ull.ValidUntil >= dateTo) ||
                    (!dateFrom.HasValue && dateTo >= ull.ValidSince && dateTo >= ull.ValidUntil) ||
                    (!ull.ValidUntil.HasValue && ull.ValidSince >= dateFrom && ull.ValidSince >= dateTo) ||
                    (!dateTo.HasValue && dateFrom >= ull.ValidSince && dateFrom >= ull.ValidUntil) ||
                    (ull.ValidSince < dateTo && dateFrom < ull.ValidUntil)
                )
            , cancellationToken);
    }

    private async Task SetEndDateForPreviousLimitIfDontHaveAsync(DateTimeOffset validSince,
        CancellationToken cancellationToken)
    {
        var limitWithoutEndDateBeforeThisLimit = await dbContext.UserLeaveLimits
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

    private async Task SetStartDateForNextLimitIfDontHaveAsync(DateTimeOffset validUntil,
        CancellationToken cancellationToken)
    {
        var limitWithoutStartDateAfterThisLimit = await dbContext.UserLeaveLimits
            .LastOrDefaultAsync(
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

    [HttpPut]
    public async Task<IActionResult> Put(
        [FromODataUri] Guid key,
        [FromBody] Delta<UserLeaveLimit> delta,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new BadHttpRequestException(InvalidModelMessage);
        }
        var updatedEntity = await crudService.PutAsync(key, delta, cancellationToken);
        await SetStartAndEndDateForNeighbourLimitsIfDontHaveAsync(delta, cancellationToken, updatedEntity);
        return Updated(updatedEntity);
    }

    private async Task SetStartAndEndDateForNeighbourLimitsIfDontHaveAsync(
        Delta<UserLeaveLimit> delta, 
        CancellationToken cancellationToken,
        UserLeaveLimit updatedEntity)
    {
        if (delta.GetChangedPropertyNames().Contains("ValidSince") && updatedEntity.ValidSince.HasValue)
        {
            await SetEndDateForPreviousLimitIfDontHaveAsync(updatedEntity.ValidSince.Value, cancellationToken);
        }
        if (delta.GetChangedPropertyNames().Contains("ValidUntil") && updatedEntity.ValidUntil.HasValue)
        {
            await SetEndDateForPreviousLimitIfDontHaveAsync(updatedEntity.ValidUntil.Value, cancellationToken);

        }
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
        var updatedEntity = await crudService.PutAsync(key, delta, cancellationToken);
        await SetStartAndEndDateForNeighbourLimitsIfDontHaveAsync(delta, cancellationToken, updatedEntity);
        return Updated(updatedEntity);
    }
}