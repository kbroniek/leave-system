using FluentAssertions;
using Marten.Linq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveSystem.UnitTests.TestHelpers;

internal class MartenQueryableStub<T> : List<T>, IMartenQueryable<T>
{
    private readonly Mock<IQueryProvider> queryProviderMock = new();
    public Type ElementType => typeof(T);

    public Expression Expression => Expression.Constant(this);

    public IQueryProvider Provider
    {
        get
        {
            queryProviderMock
                .Setup(x => x.CreateQuery<T>(It.IsAny<Expression>()))
                .Returns(this);
            return queryProviderMock.Object;
        }
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
