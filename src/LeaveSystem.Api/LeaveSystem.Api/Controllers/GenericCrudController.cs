using GoldenEye.Objects.General;
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
    public abstract class GenericCrudController<TEntity> : ODataController where TEntity : class, IHaveId<Guid>
    {
        private readonly LeaveSystemDbContext dbContext;

        public GenericCrudController(LeaveSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<TEntity>? Get() => dbContext.Set<TEntity>();

        [HttpGet("{key}")]
        [EnableQuery]
        public SingleResult<TEntity> Get([FromODataUri] Guid key)
        {
            IQueryable<TEntity> result = GetSet().Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TEntity TEntity, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            GetSet().Add(TEntity);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Created(TEntity);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] Guid key, [FromBody] Delta<TEntity> update, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await GetSet().FindAsync(new object[] { key }, cancellationToken);
            if (entity == null)
            {
                return NotFound();
            }
            update.Patch(entity);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(key, cancellationToken))
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
        public async Task<IActionResult> Put([FromODataUri] Guid key, [FromBody] TEntity update, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.Id)
            {
                return BadRequest();
            }
            dbContext.Entry(update).State = EntityState.Modified;
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(key, cancellationToken))
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
        public async Task<IActionResult> Delete([FromODataUri] Guid key, CancellationToken cancellationToken = default)
        {
            var product = await GetSet().FindAsync(new object[] { key }, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }
            GetSet().Remove(product);
            await dbContext.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        private Task<bool> ProductExists(Guid key, CancellationToken cancellationToken)
        {
            return GetSet().AnyAsync(l => l.Id == key, cancellationToken);
        }

        private DbSet<TEntity> GetSet()
        {
            var dbSet = dbContext.Set<TEntity>();
            if (dbSet == null)
            {
                throw new InvalidOperationException($"The {typeof(TEntity)} table in the {nameof(LeaveSystemDbContext)} is null.");
            }
            return dbSet;
        }
    }
}
