using FluentValidation;
using GoldenEye.Objects.General;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Db;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Api.Controllers;

public class GenericCrudService<TEntity, TId>
    where TId : IComparable<TId>
    where TEntity : class, IHaveId<TId>
{
    private readonly LeaveSystemDbContext dbContext;
    private readonly DeltaValidator<TEntity> deltaValidator;
    private readonly IValidator<TEntity> entityValidator;
    private const string NotFoundMessage = "Entity Not Found";
    public GenericCrudService(
        LeaveSystemDbContext dbContext, 
        DeltaValidator<TEntity> deltaValidator, 
        IValidator<TEntity> entityValidator)
    {
        this.dbContext = dbContext;
        this.deltaValidator = deltaValidator;
        this.entityValidator = entityValidator;
    }
        
    public virtual IQueryable<TEntity>? Get() => dbContext.Set<TEntity>();
        
    public IQueryable<TEntity> GetAsQueryable(TId key) 
        => GetSet().Where(p => p.Id.CompareTo(key) == 0);
        
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await entityValidator.ValidateAsync(entity, o =>
            {
                o.ThrowOnFailures();
            }, cancellationToken);
        GetSet().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }
        
    public virtual async Task<TEntity> PatchAsync(TId key, Delta<TEntity> update, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(key, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(NotFoundMessage);
        }
        var validatedDelta = deltaValidator.CreateDeltaWithoutProtectedProperties(update);
        validatedDelta.Patch(entity);
        await entityValidator.ValidateAsync(entity, o =>
        {
            o.ThrowOnFailures();
        }, cancellationToken);
        dbContext.Entry(entity).State = EntityState.Modified;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExists(key, cancellationToken))
            {
                throw new EntityNotFoundException(NotFoundMessage);
            }
            throw;
        }
        return entity;
    }
        
    public async Task<TEntity> PutAsync(TId key, Delta<TEntity> update, CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(key, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(NotFoundMessage);
        }
        var validatedDelta = deltaValidator.CreateDeltaWithoutProtectedProperties(update);
        validatedDelta.Put(entity);
        await entityValidator.ValidateAsync(entity, o =>
        {
            o.ThrowOnFailures();
        }, cancellationToken);
        dbContext.Entry(entity).State = EntityState.Modified;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExists(key, cancellationToken))
            {
                throw new EntityNotFoundException(NotFoundMessage);
            }
            throw;
        }
        return entity;
    }

    public async Task DeleteAsync(TId key, CancellationToken cancellationToken = default)
    {
        var product = await GetSet().FindAsync(new object[] { key }, cancellationToken);
        if (product == null)
        {
            throw new EntityNotFoundException(NotFoundMessage);
        }
        GetSet().Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TEntity?> FindAsync(TId key, CancellationToken cancellationToken = default)
        => await GetSet().FindAsync(new object[] { key }, cancellationToken);
        
    private Task<bool> ProductExists(TId key, CancellationToken cancellationToken)
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

public class DeltaValidator<TEntity> where TEntity : class
{
    private readonly string[] protectedPropertyNames;

    public DeltaValidator(params string[] protectedPropertyNames)
    {
        this.protectedPropertyNames = protectedPropertyNames;
    }

    public virtual Delta<TEntity> CreateDeltaWithoutProtectedProperties(Delta<TEntity> delta)
    {
        var deltaWithoutProtectedProperties = new Delta<TEntity>();
        foreach (var propertyName in delta.GetChangedPropertyNames())
        {
            if (!protectedPropertyNames.Contains(propertyName) 
                && delta.TryGetPropertyValue(propertyName, out var propertyValue))
            {
                deltaWithoutProtectedProperties.TrySetPropertyValue(propertyName, propertyValue);
            }
        }
        return deltaWithoutProtectedProperties;
    }
}
