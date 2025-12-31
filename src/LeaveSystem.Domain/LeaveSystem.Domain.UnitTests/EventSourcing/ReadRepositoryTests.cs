namespace LeaveSystem.Domain.UnitTests.EventSourcing;

using System.Net;
using System.Threading;
using LeaveSystem.Domain.EventSourcing;
using Microsoft.Extensions.Logging;
using Moq;

public class ReadRepositoryTests
{
    private readonly Mock<IReadEventsRepository> mockReadEventsRepository = new();
    private readonly Mock<ILogger<ReadService>> mockLogger = new();
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly ReadService readService;

    public ReadRepositoryTests() =>
        readService = new ReadService(mockReadEventsRepository.Object, mockLogger.Object);

    #region FindById Tests

    [Fact]
    public async Task FindById_ShouldReturnSuccess_WhenEventIsFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var @event = new Mock<IEvent>();

        mockReadEventsRepository
            .Setup(r => r.ReadStreamAsync(id, cancellationToken))
            .Returns(AsyncEnumerable.Repeat(@event.Object, 1));

        // Act
        var result = await readService.FindById<FakeEventSource>(id, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Version);
        mockReadEventsRepository.Verify(r => r.ReadStreamAsync(id, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task FindById_ShouldReturnNotFoundError_WhenNoEventIsFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        mockReadEventsRepository
            .Setup(r => r.ReadStreamAsync(id, cancellationToken))
            .Returns(AsyncEnumerable.Empty<IEvent>());

        // Act
        var result = await readService.FindById<FakeEventSource>(id, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Error.HttpStatusCode);
    }

    #endregion

    #region FindByIds Tests

    [Fact]
    public async Task FindByIds_ShouldReturnSuccess_WhenEventsAreFound()
    {
        // Arrange
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var events = new FakeEvent[] { new(ids[0]), new(ids[1]) };

        mockReadEventsRepository
            .Setup(r => r.ReadStreamAsync(ids, cancellationToken))
            .Returns(events.ToAsyncEnumerable());

        // Act
        var result = await readService.FindByIds<FakeEventSource>(ids, cancellationToken);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal(1, result.ElementAt(0).Version);
        Assert.Equal(1, result.ElementAt(1).Version);
        mockReadEventsRepository.Verify(r => r.ReadStreamAsync(ids, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task FindByIds_ShouldReturnEmpty_WhenNoEventsAreFound()
    {
        // Arrange
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };

        mockReadEventsRepository
            .Setup(r => r.ReadStreamAsync(ids, cancellationToken))
            .Returns(AsyncEnumerable.Empty<IEvent>());

        // Act
        var result = await readService.FindByIds<FakeEventSource>(ids, cancellationToken);

        // Assert
        Assert.Empty(result);
        mockReadEventsRepository.Verify(r => r.ReadStreamAsync(ids, cancellationToken), Times.Once);
    }

    #endregion

    private record FakeEvent(Guid StreamId) : IEvent
    {
        public DateTimeOffset CreatedDate => DateTimeOffset.Parse("2023-12-15T09:40:41.8087272+00:00");
    }
}
