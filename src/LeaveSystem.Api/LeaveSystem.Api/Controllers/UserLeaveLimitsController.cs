using FluentValidation;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
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
    public async Task<IActionResult> Post([FromBody] AddUserLeaveLimitDto dto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            throw new BadHttpRequestException(InvalidModelMessage);
        }
        var leaveTypeExists = await dbContext.LeaveTypes.AnyAsync(lt => lt.Id == dto.LeaveTypeId, cancellationToken);
        if (!leaveTypeExists)
        {
            throw new EntityNotFoundException("Leave type with provided Id not exists");
        }
        var entity1 = new UserLeaveLimit()
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
        await limitValidator.ValidateAsync(entity1, cancellationToken);
        var addedEntity = await crudService.AddAsync(entity1, cancellationToken);
        return Created(addedEntity);
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
        return Updated(await crudService.PutAsync(key, delta, cancellationToken));
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
        return Updated(await crudService.PatchAsync(key, delta, cancellationToken));
    }
}