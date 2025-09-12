namespace LeaveSystem.Domain.UnitTests.EventSourcing;
using System;
using System.Net;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;
using Moq;

public class WriteRepositoryTests
{
    private readonly Mock<IAppendEventRepository> mockAppendEventRepository = new();
    private readonly WriteService writeService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly Guid id = Guid.NewGuid();

    public WriteRepositoryTests() =>
        writeService = new WriteService(mockAppendEventRepository.Object);

    [Fact]
    public async Task Write_ShouldReturnEventSource_WhenAllEventsAppendedSuccessfully()
    {
        // Arrange
        var eventSource = new FakeEventSource();
        eventSource.PendingEvents.Enqueue(new FakeEvent { StreamId = id, CreatedDate = DateTimeOffset.UtcNow });
        eventSource.PendingEvents.Enqueue(new FakeEvent { StreamId = id, CreatedDate = DateTimeOffset.UtcNow });

        mockAppendEventRepository
            .Setup(repo => repo.AppendToStreamAsync(It.IsAny<FakeEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Default); // Simulate successful append

        // Act
        var result = await writeService.Write(eventSource, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(eventSource, result.Value);
        Assert.Empty(eventSource.PendingEvents);  // Ensure all events are dequeued
    }

    [Fact]
    public async Task Write_ShouldReturnError_WhenAnEventAppendFails()
    {
        // Arrange
        var eventSource = new FakeEventSource();
        eventSource.PendingEvents.Enqueue(new FakeEvent { StreamId = id, CreatedDate = DateTimeOffset.UtcNow });

        var expectedError = new Error("Append failed", HttpStatusCode.InternalServerError, "APPEND_FAILED");
        mockAppendEventRepository
            .Setup(repo => repo.AppendToStreamAsync(It.IsAny<FakeEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedError);  // Simulate failed append

        // Act
        var result = await writeService.Write(eventSource, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.Error);
        Assert.Single(eventSource.PendingEvents);  // Ensure the event was not dequeued
    }

    [Fact]
    public async Task Write_ShouldNotAppend_WhenThereAreNoPendingEvents()
    {
        // Arrange
        var eventSource = new FakeEventSource();  // No pending events

        // Act
        var result = await writeService.Write(eventSource, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(eventSource, result.Value);
        mockAppendEventRepository.Verify(repo => repo.AppendToStreamAsync(It.IsAny<FakeEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
