using GoldenEye.Backend.Core.Exceptions;
using LeaveSystem.Api.Endpoints.Roles;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Api.UnitTests.Providers;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using Moq;

namespace LeaveSystem.Api.UnitTests.Endpoints.Roles;
public class UserRolesGraphServiceTest
{
    [Fact]
    public async Task WhenGet_ThenReturnGraphUsers()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.SetupUserCollectionResponse()
            .ReturnsAsync(new UserCollectionResponse
            {
                Value = new List<User>
                {
                    new() {
                        Id = "6ccc8c8d-df83-44d6-a6b0-fce65d1118fb",
                        AdditionalData = {
                            { TestData.FakeRoleAttributeName, TestData.FakeRolesJson }
                        }
                    },
                    new() {
                        Id = "6ccc8c8d-df83-44d6-a6b0-fce65d1118fc"
                    }
                }
            });
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new UserRolesGraphService(graphClientFactoryMock.Object, rolesAttributeNameResolver);
        //When
        var result = await sut.Get(CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(new[]
        {
            new
            {
                Id = "6ccc8c8d-df83-44d6-a6b0-fce65d1118fb",
                Roles = new string[]
                {
                    "nulla",
                    "aliquip",
                    "amet",
                    "aliqua",
                    "magna",
                    "cillum",
                    "excepteur"
                }
            },
            new
            {
                Id = "6ccc8c8d-df83-44d6-a6b0-fce65d1118fc",
                Roles = Array.Empty<string>()
            },
        }, o => o.ExcludingMissingMembers());
    }

    [Fact]
    public async Task WhenGet_CannotFind_ThenThrowError()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.SetupUserCollectionResponse()
            .ReturnsAsync((UserCollectionResponse?)null);
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new UserRolesGraphService(graphClientFactoryMock.Object, rolesAttributeNameResolver);
        //When
        var act = async () => await sut.Get(CancellationToken.None);
        //Then
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("UserCollectionResponse with id: Can't find users in graph api was not found.");
    }

    [Fact]
    public async Task WhenUpdate_ThenSendPatchRequest()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.Setup(adapter => adapter.SendAsync<User>(
            It.Is<RequestInformation>(info => info.HttpMethod == Method.PATCH),
            User.CreateFromDiscriminatorValue,
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null)
            .Verifiable(Times.Once);
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new UserRolesGraphService(graphClientFactoryMock.Object, rolesAttributeNameResolver);
        //When
        await sut.Update("6ccc8c8d-df83-44d6-a6b0-fce65d1118fd", new string[] { "test" }, CancellationToken.None);
        //Then
        mockRequestAdapter.VerifyAll();
    }
}
