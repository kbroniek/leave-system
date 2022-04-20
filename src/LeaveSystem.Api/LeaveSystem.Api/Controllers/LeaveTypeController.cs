using LeaveSystem.Api.Domains;
using LeaveSystem.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Api.Controllers
{
    [Route("api/[controller]")]
    public class LeaveTypeController : ODataController
    {
        private readonly LeaveSystemDbContext dbContext;

        public LeaveTypeController(LeaveSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<LeaveType>? Get() => dbContext.LeaveTypes;

        [HttpGet("{key}")]
        [EnableQuery]
        public SingleResult<LeaveType> Get([FromODataUri] Guid key)
        {
            IQueryable<LeaveType> result = GetLeaveTypes().Where(p => p.LeaveTypeId == key);
            return SingleResult.Create(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(LeaveType leaveType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            GetLeaveTypes().Add(leaveType);
            await dbContext.SaveChangesAsync();
            return Created(leaveType);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] Guid key, Delta<LeaveType> create)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await GetLeaveTypes().FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            create.Patch(entity);
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] Guid key, LeaveType update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.LeaveTypeId)
            {
                return BadRequest();
            }
            dbContext.Entry(update).State = EntityState.Modified;
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] int key)
        {
            var product = await GetLeaveTypes().FindAsync(key);
            if (product == null)
            {
                return NotFound();
            }
            GetLeaveTypes().Remove(product);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        private Task<bool> ProductExists(Guid key)
        {
            return GetLeaveTypes().AnyAsync(l => l.LeaveTypeId == key);
        }

        private DbSet<LeaveType> GetLeaveTypes()
        {
            if (dbContext.LeaveTypes == null)
            {
                throw new InvalidOperationException($"The {nameof(dbContext.LeaveTypes)} table in the {nameof(LeaveSystemDbContext)} is null.");
            }
            return dbContext.LeaveTypes;
        }
    }
}
