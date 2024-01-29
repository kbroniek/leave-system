using LeaveSystem.Domain.Users;
using LeaveSystem.GraphApi;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.TestDataGenerators;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Moq;

namespace LeaveSystem.UnitTests.Domain.Roles;
public class SaveGraphUserServiceTest
{
    [Fact]
    public async Task WhenUpdate_ThenSendPatchRequest()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.SetupUserResponse(Method.PATCH)
            .ReturnsAsync((User?)null)
            .Verifiable(Times.Once);
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = SaveGraphUserService.Create(graphClientFactoryMock.Object, rolesAttributeNameResolver, "fakePass", "faleIssuer");
        //When
        var result = await sut.Update("6ccc8c8d-df83-44d6-a6b0-fce65d1118fd", "fakeEmail", "fakeName", TestData.FakeRoles, CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(
            new FederatedUser("6ccc8c8d-df83-44d6-a6b0-fce65d1118fd", "fakeEmail", "fakeName", TestData.FakeRoles));
        mockRequestAdapter.VerifyAll();
    }

    [Fact]
    public async Task WhenAdd_ThenSendPostRequest()
    {
        //Given
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var mockRequestAdapter = RequestAdapterMockProvider.Create();
        var graphClient = new GraphServiceClient(mockRequestAdapter.Object);

        mockRequestAdapter.SetupUserResponse(Method.POST)
            .ReturnsAsync(new User
            {
                Id = "d0bd91a2-3d65-4b35-96c1-9e3cdce54823",
                Mail = "fake@test.com",
                DisplayName = "fakeName",
                AdditionalData = {
                    { TestData.FakeRoleAttributeName, TestData.FakeRolesJson }
                }
            });
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClient);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = SaveGraphUserService.Create(graphClientFactoryMock.Object, rolesAttributeNameResolver, "fakePass", "faleIssuer");
        //When
        var result = await sut.Add("fake@test.com", "fakeName", TestData.FakeRoles, CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(
            new FederatedUser("d0bd91a2-3d65-4b35-96c1-9e3cdce54823", "fake@test.com", "fakeName",
                TestData.FakeRoles));
    }
}
