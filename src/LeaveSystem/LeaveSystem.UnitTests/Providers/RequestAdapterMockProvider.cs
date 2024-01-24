using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Kiota.Serialization.Json;
using Moq;
using Moq.Language.Flow;

namespace LeaveSystem.UnitTests.Providers;

public static class RequestAdapterMockProvider
{
    public static Mock<IRequestAdapter> Create(MockBehavior mockBehavior = MockBehavior.Strict)
    {
        var mockSerializationWriterFactory = new Mock<ISerializationWriterFactory>();
        mockSerializationWriterFactory.Setup(factory => factory.GetSerializationWriter(It.IsAny<string>()))
            .Returns((string _) => new JsonSerializationWriter());

        var mockRequestAdapter = new Mock<IRequestAdapter>(mockBehavior);
        mockRequestAdapter.SetupGet(adapter => adapter.BaseUrl).Returns((string?)null);
        mockRequestAdapter.SetupSet(adapter => adapter.BaseUrl = It.IsAny<string>());
        mockRequestAdapter.Setup(adapter => adapter.EnableBackingStore(It.IsAny<IBackingStoreFactory>()));
        mockRequestAdapter.SetupGet(adapter => adapter.SerializationWriterFactory).Returns(mockSerializationWriterFactory.Object);

        return mockRequestAdapter;
    }

    public static ISetup<IRequestAdapter, Task<UserCollectionResponse?>> SetupUserCollectionResponse(this Mock<IRequestAdapter> mockRequestAdapter) =>
        mockRequestAdapter.Setup(adapter => adapter.SendAsync<UserCollectionResponse>(
            It.Is<RequestInformation>(info => info.HttpMethod == Method.GET),
            UserCollectionResponse.CreateFromDiscriminatorValue,
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()));
    public static ISetup<IRequestAdapter, Task<User?>> SetupUserResponse(this Mock<IRequestAdapter> mockRequestAdapter, Method method = Method.GET) =>
        mockRequestAdapter.Setup(adapter => adapter.SendAsync<User>(
            It.Is<RequestInformation>(info => info.HttpMethod == method),
            User.CreateFromDiscriminatorValue,
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()));
}
