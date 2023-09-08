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
    public abstract class GenericCrudController<TEntity, TId> : ODataController
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
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
        public SingleResult<TEntity> Get([FromODataUri] TId key)
        {
            IQueryable<TEntity> result = GetSet().Where(p => p.Id.CompareTo(key) == 0);
            return SingleResult.Create(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TEntity entity, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            GetSet().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Created(entity);
        }

        [HttpPatch]
        public async Task<IActionResult> Patch([FromODataUri] TId key, [FromBody] Delta<TEntity> update, CancellationToken cancellationToken = default)
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
                if (!await IsEntityExists(key, cancellationToken))
                {
                    return NotFound();
                }
                throw;
            }
            return Updated(entity);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromODataUri] TId key, [FromBody] TEntity update, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key.CompareTo(update.Id) != 0)
            {
                return BadRequest("Id in the path is not the same as in the body.");
            }
            var local = GetSet()
                .Local
                .FirstOrDefault(entry => entry.Id.Equals(key));
            if (local != null)
            {
                dbContext.Entry(local).State = EntityState.Detached;
            }
            dbContext.Entry(update).State = EntityState.Modified;
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await IsEntityExists(key, cancellationToken))
                {
                    return NotFound("Entity doesn't exist.");
                }
                throw;
            }
            return Updated(update);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromODataUri] TId key, CancellationToken cancellationToken = default)
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

        private Task<bool> IsEntityExists(TId key, CancellationToken cancellationToken)
        {
            return GetSet().AnyAsync(l => l.Id.CompareTo(key) == 0, cancellationToken);
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
