namespace LeaveSystem.Functions.UnitTests.EventSourcing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Functions.EventSourcing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;

public class EventRepositoryTests
{
    private readonly Mock<CosmosClient> mockCosmosClient = new();
    private readonly Mock<Container> mockContainer = new();
    private readonly Mock<ILogger<EventRepository>> mockLogger = new();
    private readonly string databaseName = "TestDatabase";
    private readonly string eventsContainerName = "TestContainer";
    private readonly EventRepository eventRepository;
    private readonly CancellationToken cancellationToken = CancellationToken.None;

    public EventRepositoryTests()
    {
        eventRepository = new EventRepository(mockCosmosClient.Object, mockLogger.Object, databaseName, eventsContainerName);
        mockCosmosClient
            .Setup(client => client.GetContainer(databaseName, eventsContainerName))
            .Returns(mockContainer.Object);
    }

    [Fact]
    public async Task AppendToStreamAsync_ShouldReturnSuccess_WhenEventIsAppendedSuccessfully()
    {
        // Arrange
        var @event = new Mock<IEvent>();
        @event.Setup(e => e.StreamId).Returns(Guid.NewGuid());

        mockContainer
            .Setup(container => container.CreateItemAsync(It.IsAny<EventModel<object>>(), It.IsAny<PartitionKey>(), null, cancellationToken))
            .ReturnsAsync(Mock.Of<ItemResponse<EventModel<object>>>());  // Simulate successful creation

        // Act
        var result = await eventRepository.AppendToStreamAsync(@event.Object, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AppendToStreamAsync_ShouldReturnConflictError_WhenCosmosExceptionOccursWithConflictStatusCode()
    {
        // Arrange
        var @event = new Mock<IEvent>();
        @event.Setup(e => e.StreamId).Returns(Guid.NewGuid());

        mockContainer
            .Setup(container => container.CreateItemAsync(It.IsAny<EventModel<object>>(), It.IsAny<PartitionKey>(), null, cancellationToken))
            .ThrowsAsync(new CosmosException("Conflict", HttpStatusCode.Conflict, 0, "", 0));  // Simulate conflict

        // Act
        var result = await eventRepository.AppendToStreamAsync(@event.Object, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.Error.HttpStatusCode);
        Assert.Equal("This event already exists", result.Error.Message);
    }

    [Fact]
    public async Task AppendToStreamAsync_ShouldReturnInternalServerError_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var @event = new Mock<IEvent>();
        @event.Setup(e => e.StreamId).Returns(Guid.NewGuid());

        mockContainer
            .Setup(container => container.CreateItemAsync(It.IsAny<EventModel<object>>(), It.IsAny<PartitionKey>(), null, cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await eventRepository.AppendToStreamAsync(@event.Object, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Error.HttpStatusCode);
        Assert.Equal("Unexpected error occurred while insert data to DB", result.Error.Message);
    }

    [Fact]
    public async Task ReadStreamAsync_ShouldReturnEvents_WhenEventsAreFoundInTheStream()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        IEnumerable<EventModel<JToken>> events = new List<EventModel<JToken>>
        {
            new(Guid.NewGuid(), streamId,
            JToken.FromObject(new FakeEvent { StreamId = streamId }), typeof(FakeEvent).AssemblyQualifiedName!)
        };

        var feedIterator = new Mock<FeedIterator<EventModel<JToken>>>();
        feedIterator.SetupSequence(iterator => iterator.HasMoreResults)
            .Returns(true)
            .Returns(false);
        feedIterator.Setup(iterator => iterator.ReadNextAsync(cancellationToken))
            .ReturnsAsync(Mock.Of<FeedResponse<EventModel<JToken>>>(response => response.GetEnumerator() == events.GetEnumerator()));

        mockContainer
            .Setup(container => container.GetItemQueryIterator<EventModel<JToken>>(It.IsAny<string>(), null, It.IsAny<QueryRequestOptions>()))
            .Returns(feedIterator.Object);

        // Act
        var result = eventRepository.ReadStreamAsync(streamId, cancellationToken);

        // Assert
        var eventList = new List<IEvent>();
        await foreach (var e in result)
        {
            eventList.Add(e);
        }

        Assert.Single(eventList);
        Assert.IsType<FakeEvent>(eventList[0]);
    }
}
