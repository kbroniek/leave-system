using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Extensions.Basic;
using GoldenEye.Marten.Events.Storage;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using Marten;
using Marten.Linq;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class ImpositionValidatorTest
{
    [Fact]
    public async Task WhenOtherLeveRequestOverlappingWithThisDate_ThenThrowValidationException()
    {
        //Given
        var queryableMock = new MartenQueryableMock<LeaveRequestCreated>(
            GetLeaveRequestCreatedEvents(),
            new Mock<Expression>().Object,
            new Mock<IQueryProvider>().Object
        );
        var dbContext = await DbContextFactory.CreateDbContextAsync();
        var workingHoursServiceMock = new Mock<WorkingHoursService>();
        var documentSessionMock = new Mock<IDocumentSession>();
        documentSessionMock.Setup(v => v.Events.QueryRawEventDataOnly<LeaveRequestCreated>())
            .Returns(queryableMock);
        var creatingLeaveRequest = LeaveRequestCreated.Create(
            Guid.Empty,
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            TimeSpan.FromDays(6),
            Guid.NewGuid(),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        );
        var leaveRequestCreatedEvents = GetLeaveRequestCreatedEvents().AsQueryable();
        var leaveRequestsFromDb = GetLeaveRequestsFromDb();
        var requestValidator =
            new CreateLeaveRequestValidator(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);
        //When
        var act = async () => { await requestValidator.ImpositionValidator(creatingLeaveRequest); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    private IEnumerable<LeaveRequest> GetLeaveRequestsFromDb()
    {
        return new[]
        {
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            )
        };
    }

    private LeaveRequest CreateLeaveRequestFromParams(
        Guid leaveRequestId,
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        TimeSpan duration,
        Guid leaveTypeId,
        string? remarks,
        FederatedUser createdBy)
    {
        var @event =
            LeaveRequestCreated.Create(leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, remarks, createdBy);
        return LeaveRequest.CreatePendingLeaveRequest(@event);
    }

    private List<LeaveRequestCreated> GetLeaveRequestCreatedEvents()
    {
        return new List<LeaveRequestCreated>()
        {
            LeaveRequestCreated.Create(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
        };
    }
}

public class MartenQueryableMock<T> : IMartenQueryable<T>
{
    private readonly IEnumerable<T> data;

    public MartenQueryableMock(IEnumerable<T> data, Expression expression, IQueryProvider provider)
    {
        this.data = data;
        ElementType = typeof(T);
        Expression = expression;
        Provider = provider;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Task<IReadOnlyList<TResult>> ToListAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AnyAsync(CancellationToken token)
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

    public Task<TResult> FirstAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> FirstOrDefaultAsync<TResult>(CancellationToken token)
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

    public Task<TResult> SumAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TResult> MinAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<TResult> MaxAsync<TResult>(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<double> AverageAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public QueryPlan Explain(FetchType fetchType = FetchType.FetchMany,
        Action<IConfigureExplainExpressions>? configureExplain = null)
    {
        throw new NotImplementedException();
    }

    public QueryStatistics Statistics => throw new System.NotImplementedException();
    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, Action<TInclude> callback)
        where TInclude : notnull
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> Include<TInclude>(Expression<Func<T, object>> idSource, IList<TInclude> list)
        where TInclude : notnull
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> Include<TInclude, TKey>(Expression<Func<T, object>> idSource,
        IDictionary<TKey, TInclude> dictionary) where TInclude : notnull where TKey : notnull
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> Stats(out QueryStatistics stats)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}