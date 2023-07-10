using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using Marten;
using Marten.Events;
using Marten.Linq;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class ImpositionValidatorTest
{
    [Fact]
    public async Task WhenOtherLeveRequestOverlappingWithThisDate_ThenThrowValidationException()
    {
        //Given
        var dbContext = await DbContextFactory.CreateDbContextAsync();
        var workingHoursServiceMock = new Mock<WorkingHoursService>();
        var documentSessionMock = new Mock<IDocumentSession>();
        var eventStoreMock = new Mock<IEventStore>();
        Guid fakeLeaveRequestId = Guid.NewGuid();
        LeaveRequestCreated fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            fakeLeaveRequestId,
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            TimeSpan.FromDays(6),
            Guid.NewGuid(),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        );
        var fakeLeaveRequestEntity = LeaveRequest.CreatePendingLeaveRequest(fakeLeaveRequestCreatedEvent);
        var queryableMock = new MartenList<LeaveRequestCreated>() {
            fakeLeaveRequestCreatedEvent
        };
        documentSessionMock.SetupGet(v => v.Events)
            .Returns(eventStoreMock.Object);
        eventStoreMock.Setup(v => v.QueryRawEventDataOnly<LeaveRequestCreated>())
            .Returns(queryableMock);
        eventStoreMock.Setup(v => v.AggregateStreamAsync<LeaveRequest>(
                fakeLeaveRequestId,
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<LeaveRequest>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeLeaveRequestEntity)
            .Verifiable();
        var testingLeaveRequest = LeaveRequestCreated.Create(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            TimeSpan.FromDays(6),
            Guid.NewGuid(),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        );

        //var leaveRequestCreatedEvents = GetLeaveRequestCreatedEvents().AsQueryable();
        //var leaveRequestsFromDb = GetLeaveRequestsFromDb();
        var requestValidator =
            new CreateLeaveRequestValidator(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);
        //When
        var act = async () => { await requestValidator.ImpositionValidator(testingLeaveRequest); };
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

public class MartenList<T> : List<T>, IMartenQueryable<T>
{
    private Mock<IQueryProvider> queryProviderMock = new();
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

    public Task<bool> AnyAsync(CancellationToken token) => Task.FromResult(this.Any());

    public Task<double> AverageAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(CancellationToken token) => Task.FromResult(Count);

    public Task<long> CountLongAsync(CancellationToken token) => Task.FromResult((long)Count);

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