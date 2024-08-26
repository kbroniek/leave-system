namespace LeaveSystem.Functions.UnitTests.EventSourcing;

using System.Net;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.UnitTests.testHelpers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static LeaveSystem.Functions.Config;

public class EventRepository_AppendAsyncTest
{
    [Fact]
    public async Task WhenEventConfilctedWithOtherEvent_ThenThrowException()
    {
        var eventRepositorySettings = new EventRepositorySettings { ContainerName = "fakeContainer", DatabaseName = "fakeDatabase" };
        var cosmosClient = Substitute.For<CosmosClient>("fakeConnection");
        var container = Substitute.For<Container>();
        var fakeEvent = new FakeEvent
        {
            CreatedAt = DateTimeOffset.Parse("06-12-2024"),
            Id = Guid.Parse("14b9c000-e367-45e1-a057-a000792a4adf")
        };
        container.CreateItemAsync(Arg.Is<Event>(i => AreEqual(i, fakeEvent)))
            .Throws(new CosmosException("conflict!", HttpStatusCode.Conflict, 0, "activityId", 6.0));
        cosmosClient.GetContainer(eventRepositorySettings.DatabaseName, eventRepositorySettings.ContainerName)
            .Returns(container);
        var logger = Substitute.For<ILogger<EventRepository>>();
        var sut = new EventRepository(cosmosClient, logger, eventRepositorySettings);
        var result = await sut.AppendAsync(fakeEvent);
        result.Should().;
    }

    private bool AreEqual(Event event1, Event event2) => event1.CreatedAt == event2.CreatedAt && event1.StreamId == event2.StreamId;
}
