using Ardalis.GuardClauses;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared.LeaveRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveTypesController : ODataController
    {
        private readonly LeaveSystemDbContext dbContext;
        private readonly GenericCrudService<LeaveType, Guid> crudService;
        private const string InvalidModelMessage = "This request is not valid oDataRequest";

        public LeaveTypesController(LeaveSystemDbContext dbContext, GenericCrudService<LeaveType, Guid> crudService)
        {
            this.dbContext = dbContext;
            this.crudService = crudService;
        }
        
        [HttpGet]
        [EnableQuery]
        public IQueryable<LeaveType>? Get()
        {
            return crudService.Get();
        }

        [HttpGet("{key}")]
        [EnableQuery]
        public SingleResult<LeaveType> Get([FromODataUri] Guid key)
        {
            var entity = crudService.GetAsQueryable(key);
            return SingleResult.Create(entity);
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AddLeaveTypeDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                throw new BadHttpRequestException(InvalidModelMessage);
            }
            var entity = new LeaveType
            {
                Id = Guid.NewGuid(),
                BaseLeaveTypeId = dto.BaseLeaveTypeId,
                Name = dto.Name,
                Order = dto.Order,
                Properties = new ()
                {
                    Catalog = dto.Catalog,
                    DefaultLimit = dto.DefaultLimit,
                    Color = dto.Color,
                    IncludeFreeDays = dto.IncludeFreeDays
                }
            };
            var createdEntity = await crudService.AddAsync(entity, cancellationToken);
            return Created(createdEntity);
        }
    }
}
