using Ardalis.GuardClauses;
using GoldenEye.Exceptions;
using GoldenEye.Objects.General;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using RestSharp.Validation;
using NotFoundException = GoldenEye.Exceptions.NotFoundException;

namespace LeaveSystem.Api.Controllers;

public class GenericCrudService<TEntity, TId>
    where TId : IComparable<TId>
    where TEntity : class, IHaveId<TId>
{
    private readonly LeaveSystemDbContext dbContext;
    private readonly DeltaValidator<TEntity> deltaValidator;
    private const string NotFoundMessage = "Entity Not Found";
    public GenericCrudService(LeaveSystemDbContext dbContext, DeltaValidator<TEntity> deltaValidator)
    {
        this.dbContext = dbContext;
        this.deltaValidator = deltaValidator;
    }
        
    public virtual IQueryable<TEntity>? Get() => dbContext.Set<TEntity>();
        
    public IQueryable<TEntity> GetAsQueryable(TId key) 
        => GetSet().Where(p => p.Id.CompareTo(key) == 0);
        
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
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

    public Delta<TEntity> CreateDeltaWithoutProtectedProperties(Delta<TEntity> delta)
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
