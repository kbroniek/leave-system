using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Marten.Linq;
using Moq;

namespace LeaveSystem.UnitTests.Stubs;

internal class MartenQueryableStub<T> : EnumerableQuery<T>, IMartenQueryable<T>, IQueryProvider
{
    public MartenQueryableStub(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    private MartenQueryableStub(Expression expression) : base(expression)
    {
    }

    public QueryStatistics Statistics => throw new NotImplementedException();

    public Task<bool> AnyAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<double> AverageAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<long> CountLongAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
        {
            throw new ArgumentException(nameof(expression));
        }
        return new MartenQueryableStub<TElement>(expression);
    }

    public QueryPlan Explain(FetchType fetchType = FetchType.FetchMany, Action<IConfigureExplainExpressions>? configureExplain = null)
    {
        throw new NotImplementedException();
    }

    public Task<TResult> FirstAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> FirstOrDefaultAsync<TResult>(CancellationToken token)
    {
        if (typeof(TResult) != typeof(T))
        {
            throw new Exception("TypesAreDifferent");
        }
        return Task.FromResult(this.Any() ? this.First().As<TResult>() : default);
    }

    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, Action<TInclude> callback) where TInclude : notnull
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, IList<TInclude> list) where TInclude : notnull
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> Include<TInclude, TKey>(Expression<Func<T, object>> idSource, IDictionary<TKey, TInclude> dictionary)
        where TInclude : notnull
        where TKey : notnull
    {
        throw new NotImplementedException();
    }

    public Task<TResult> MaxAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TResult> MinAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TResult> SingleAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> SingleOrDefaultAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> Stats(out QueryStatistics stats)
    {
        stats = new()
        {
            TotalResults = this.Count()
        };
        return this;
    }

    public Task<TResult> SumAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TResult>> ToListAsync<TResult>(CancellationToken token) =>
        Task.FromResult(this.ToList().AsReadOnly().As<IReadOnlyList<TResult>>());
}
