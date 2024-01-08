namespace GoldenEye.Backend.Core.Marten.Repositories.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Marten;
using GoldenEye.Backend.Core.DDD.Events;
using GoldenEye.Backend.Core.Exceptions;
using GoldenEye.Backend.Core.Marten.Events.Storage.Custom;
using GoldenEye.Backend.Core.Repositories;
using GoldenEye.Shared.Core.Objects.General;

public class MartenEventSourcedRepository<TEntity> : IRepository<TEntity>, IReadonlyRepository<TEntity> where TEntity : class, IHaveId, IEventSource, new()
{
    private readonly IDocumentSession documentSession;

    private readonly MartenEventStore eventStore;

    public MartenEventSourcedRepository(IDocumentSession documentSession, MartenEventStore eventStore)
    {
        this.documentSession = documentSession ?? throw new ArgumentException("documentSession is null", nameof(documentSession));
        this.eventStore = eventStore ?? throw new ArgumentException("eventStore is null", nameof(eventStore));
    }

    public TEntity FindById(object id)
    {
        if (!(id is Guid guid))
        {
            throw new NotSupportedException("Id of the Event Sourced aggregate has to be Guid");
        }

        if (documentSession.Events.FetchStreamState(guid) == null)
        {
            return null;
        }

        return eventStore.Aggregate<TEntity>(guid);
    }

    public async Task<TEntity> FindByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!(id is Guid guidId))
        {
            throw new NotSupportedException("Id of the Event Sourced aggregate has to be Guid");
        }

        return (await documentSession.Events.FetchStreamStateAsync(guidId, cancellationToken) == null) ? null : (await eventStore.AggregateAsync<TEntity>(guidId, 0, null, cancellationToken));
    }

    public TEntity GetById(object id)
    {
        return FindById(id) ?? throw NotFoundException.For<TEntity>(id);
    }

    public async Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken))
    {
        return (await FindByIdAsync(id, cancellationToken)) ?? throw NotFoundException.For<TEntity>(id);
    }

    public IQueryable<TEntity> Query()
    {
        return documentSession.Query<TEntity>();
    }

    public IReadOnlyCollection<TEntity> Query(string query, params object[] queryParams)
    {
        if (query == null)
        {
            throw new ArgumentNullException("query");
        }

        if (queryParams == null)
        {
            throw new ArgumentNullException("queryParams");
        }

        return documentSession.Query<TEntity>(query, queryParams);
    }

    public async Task<IReadOnlyCollection<TEntity>> QueryAsync(string query, CancellationToken cancellationToken = default(CancellationToken), params object[] queryParams)
    {
        if (query == null)
        {
            throw new ArgumentNullException("query");
        }

        if (queryParams == null)
        {
            throw new ArgumentNullException("queryParams");
        }

        return await documentSession.QueryAsync<TEntity>(query, cancellationToken, queryParams);
    }

    public TEntity Add(TEntity entity)
    {
        return Store(entity);
    }

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        return StoreAsync(entity, cancellationToken);
    }

    public IReadOnlyCollection<TEntity> AddAll(params TEntity[] entities)
    {
        if (entities == null)
        {
            throw new ArgumentNullException("entities");
        }

        if (entities.Length == 0)
        {
            throw new ArgumentOutOfRangeException("entities", entities.Length, "AddAll needs to have at least one entity provided.");
        }

        documentSession.Insert<TEntity>(entities);
        return (IReadOnlyCollection<TEntity>)(object)entities;
    }

    public Task<IReadOnlyCollection<TEntity>> AddAllAsync(CancellationToken cancellationToken = default(CancellationToken), params TEntity[] entities)
    {
        return Task.FromResult(AddAll(entities));
    }

    public TEntity Update(TEntity entity)
    {
        return Store(entity);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
    {
        return StoreAsync(entity, cancellationToken);
    }

    public TEntity Delete(TEntity entity)
    {
        return Store(entity);
    }

    public Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
    {
        return StoreAsync(entity, cancellationToken);
    }

    public bool DeleteById(object id)
    {
        throw new NotImplementedException("DeleteById is not supported by Event Source repository. Use method with entity object.");
    }

    public Task<bool> DeleteByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException("DeleteByIdAsync is not supported by Event Source repository. Use method with entity object.");
    }

    public void SaveChanges()
    {
        documentSession.SaveChanges();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        return documentSession.SaveChangesAsync(cancellationToken);
    }

    private TEntity Store(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        documentSession.Delete<TEntity>(entity);
        return entity;
    }

    private async Task<TEntity> StoreAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        await eventStore.StoreAsync(entity.Id, cancellationToken, entity.PendingEvents.ToArray());
        return entity;
    }
}
