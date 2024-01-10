using GoldenEye.Backend.Core.DDD.Events.Store;
using GoldenEye.Shared.Core.Objects.General;
using Marten;

namespace GoldenEye.Backend.Core.Marten.Events.Storage.Custom;

public class MartenEventStore : IEventStore
{
    public class EventProjectionStore : IEventProjectionStore
    {
        private readonly IDocumentSession documentSession;

        public EventProjectionStore(IDocumentSession documentSession)
        {
            this.documentSession = documentSession ?? throw new ArgumentException("documentSession is null", nameof(documentSession));
        }

        public IQueryable<TProjection> Query<TProjection>()
        {
            return documentSession.Query<TProjection>();
        }

        IQueryable<TProjection> IEventProjectionStore.CustomQuery<TProjection>(string query)
        {
            return documentSession.Query<TProjection>(query, Array.Empty<object>()).AsQueryable();
        }

        TProjection IEventProjectionStore.GetById<TProjection>(Guid id)
        {
            return Query<TProjection>().SingleOrDefault((TProjection p) => p.Id == id);
        }

        Task<TProjection> IEventProjectionStore.GetByIdAsync<TProjection>(Guid id, CancellationToken cancellationToken)
        {
            return Query<TProjection>().SingleOrDefaultAsync((TProjection p) => p.Id == id, cancellationToken);
        }
    }

    private readonly IDocumentSession documentSession;

    public IEventProjectionStore Projections { get; }

    public MartenEventStore(IDocumentSession documentSession)
    {
        this.documentSession = documentSession ?? throw new ArgumentException("documentSession is null", nameof(documentSession));
        Projections = new EventProjectionStore(documentSession);
    }

    public void SaveChanges()
    {
        documentSession.SaveChanges();
    }

    public Task SaveChangesAsync(CancellationToken token = default(CancellationToken))
    {
        return documentSession.SaveChangesAsync(token);
    }

    public Guid Store(Guid streamId, params GoldenEye.Backend.Core.DDD.Events.IEvent[] events)
    {
        return documentSession.Events.Append(streamId, events).Id;
    }

    public Guid Store(Guid streamId, int version, params GoldenEye.Backend.Core.DDD.Events.IEvent[] events)
    {
        return documentSession.Events.Append(streamId, version, events).Id;
    }

    public Task<Guid> StoreAsync(Guid streamId, params GoldenEye.Backend.Core.DDD.Events.IEvent[] events)
    {
        return Task.FromResult(Store(streamId, events));
    }

    public Task<Guid> StoreAsync(Guid streamId, int version, params GoldenEye.Backend.Core.DDD.Events.IEvent[] events)
    {
        return Task.FromResult(Store(streamId, version, events));
    }

    public Task<Guid> StoreAsync(Guid streamId, CancellationToken cancellationToken = default(CancellationToken), params GoldenEye.Backend.Core.DDD.Events.IEvent[] events)
    {
        return Task.FromResult(Store(streamId, events));
    }

    public Task<Guid> StoreAsync(Guid streamId, int version, CancellationToken cancellationToken = default(CancellationToken), params GoldenEye.Backend.Core.DDD.Events.IEvent[] events)
    {
        return Task.FromResult(Store(streamId, version, events));
    }

    public TEntity Aggregate<TEntity>(Guid streamId, int version = 0, DateTime? timestamp = null) where TEntity : class, new()
    {
        return documentSession.Events.AggregateStream<TEntity>(streamId, version, timestamp, (TEntity)null);
    }

    public Task<TEntity> AggregateAsync<TEntity>(Guid streamId, int version = 0, DateTime? timestamp = null, CancellationToken cancellationToken = default(CancellationToken)) where TEntity : class, new()
    {
        return documentSession.Events.AggregateStreamAsync<TEntity>(streamId, version, timestamp, (TEntity)null, 0, cancellationToken);
    }

    public Task<TEntity> AggregateAsync<TEntity>(Guid streamId, CancellationToken cancellationToken = default(CancellationToken), int version = 0, DateTime? timestamp = null) where TEntity : class, new()
    {
        return documentSession.Events.AggregateStreamAsync<TEntity>(streamId, version, timestamp, (TEntity)null, 0, cancellationToken);
    }

    public TEvent FindById<TEvent>(Guid id) where TEvent : class, GoldenEye.Backend.Core.DDD.Events.IEvent, IHaveGuidId
    {
        var @event = documentSession.Events.Load<TEvent>(id);
        if (@event == null)
        {
            return null;
        }

        return @event.Data;
    }

    public async Task<TEvent> FindByIdAsync<TEvent>(Guid id, CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class, GoldenEye.Backend.Core.DDD.Events.IEvent, IHaveGuidId
    {
        var obj = await documentSession.Events.LoadAsync<TEvent>(id, cancellationToken);
        return (obj != null) ? obj.Data : null;
    }

    public IList<GoldenEye.Backend.Core.DDD.Events.IEvent> Query(Guid? streamId = null, int? version = null, DateTime? timestamp = null)
    {
        return (from ev in Filter(streamId, version, timestamp).ToList()
                select ev.Data).OfType<GoldenEye.Backend.Core.DDD.Events.IEvent>().ToList();
    }

    public Task<IList<GoldenEye.Backend.Core.DDD.Events.IEvent>> QueryAsync(Guid? streamId = null, int? version = null, DateTime? timestamp = null)
    {
        return QueryAsync(default(CancellationToken), streamId, version, timestamp);
    }

    public async Task<IList<GoldenEye.Backend.Core.DDD.Events.IEvent>> QueryAsync(CancellationToken cancellationToken = default(CancellationToken), Guid? streamId = null, int? version = null, DateTime? timestamp = null)
    {
        return (await Filter(streamId, version, timestamp).ToListAsync(cancellationToken)).Select((global::Marten.Events.IEvent ev) => ev.Data).OfType<GoldenEye.Backend.Core.DDD.Events.IEvent>().ToList();
    }

    public IList<TEvent> Query<TEvent>(Guid? streamId = null, int? version = null, DateTime? timestamp = null) where TEvent : class, GoldenEye.Backend.Core.DDD.Events.IEvent
    {
        return Query(streamId, version, timestamp).OfType<TEvent>().ToList();
    }

    public Task<IList<TEvent>> QueryAsync<TEvent>(Guid? streamId = null, int? version = null, DateTime? timestamp = null) where TEvent : class, GoldenEye.Backend.Core.DDD.Events.IEvent
    {
        return QueryAsync<TEvent>(default(CancellationToken), streamId, version, timestamp);
    }

    public async Task<IList<TEvent>> QueryAsync<TEvent>(CancellationToken cancellationToken = default(CancellationToken), Guid? streamId = null, int? version = null, DateTime? timestamp = null) where TEvent : class, GoldenEye.Backend.Core.DDD.Events.IEvent
    {
        return (await QueryAsync(cancellationToken, streamId, version, timestamp)).OfType<TEvent>().ToList();
    }

    private IQueryable<global::Marten.Events.IEvent> Filter(Guid? streamId, int? version, DateTime? timestamp)
    {
        IQueryable<global::Marten.Events.IEvent> queryable = documentSession.Events.QueryAllRawEvents().AsQueryable();
        if (streamId.HasValue)
        {
            queryable = queryable.Where((global::Marten.Events.IEvent ev) => ev.StreamId == streamId);
        }

        if (version.HasValue)
        {
            queryable = queryable.Where((global::Marten.Events.IEvent ev) => (int?)ev.Version >= version);
        }

        if (timestamp.HasValue)
        {
            queryable = queryable.Where((global::Marten.Events.IEvent ev) => ev.Timestamp >= (DateTimeOffset?)timestamp);
        }

        return queryable;
    }
}
