namespace LeaveSystem.Domain.UnitTests.EventSourcing;

using System.Net;
using LeaveSystem.Domain.EventSourcing;
using Microsoft.Extensions.Logging;
using Moq;

public class ReadRepositoryTests
{
    private readonly Mock<IReadEventsRepository> mockReadEventsRepository = new();
    private readonly Mock<ILogger<ReadService>> mockLogger = new();
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly Guid id = Guid.NewGuid();
    private readonly ReadService readService;

    public ReadRepositoryTests() =>
        readService = new ReadService(mockReadEventsRepository.Object, mockLogger.Object);

    [Fact]
    public async Task FindByIdAsync_ShouldReturnError_WhenNoEventsFound()
    {
        // Arrange
        mockReadEventsRepository
            .Setup(repo => repo.ReadStreamAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(GetAsyncEnumerable(Enumerable.Empty<FakeEvent>()));

        // Act
        var result = await readService.FindByIdAsync<FakeEventSource>(id, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Error.HttpStatusCode);
        Assert.Equal($"Cannot find the resource id {id}", result.Error.Message);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnEventSource_WhenEventsFound()
    {
        // Arrange
        var events = new List<FakeEvent>
        {
            new() { StreamId = id, CreatedDate = DateTimeOffset.UtcNow },
            new() { StreamId = id, CreatedDate = DateTimeOffset.UtcNow }
        };

        mockReadEventsRepository
            .Setup(repo => repo.ReadStreamAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(GetAsyncEnumerable(events));

        // Act
        var result = await readService.FindByIdAsync<FakeEventSource>(id, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(events.Count, result.Value.Version);  // Assuming each event increments a version.
    }

    private static async IAsyncEnumerable<T> GetAsyncEnumerable<T>(IEnumerable<T> list)
    {
        foreach (var item in list)
        {
            yield return item;
            await Task.Yield();  // Simulating async operation.
        }
    }
}
