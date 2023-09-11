using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Endpoints.Roles;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Api.UnitTests.Providers;
using LeaveSystem.Api.UnitTests.Stubs;
using LeaveSystem.Shared;
using Microsoft.Graph;
using Moq;

namespace LeaveSystem.Api.UnitTests.Endpoints.Roles;

public class UserRolesGraphServiceGetTest
{
    [Fact]
    public async Task WhenGetting_ThenReturnGraphUsers()
    {
        //Given
        var fakeRolesAttribute = JsonSerializer.Deserialize<RolesAttribute>(TestData.FakeRolesJson);
        var users = GraphServiceUsersCollectionPageProvider.Get(TestData.FakeRoleAttributeName, TestData.FakeRolesJson);
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var graphServiceUsersCollectionRequestMock = new Mock<IGraphServiceUsersCollectionRequest>();
        graphServiceUsersCollectionRequestMock.Setup(m => m.Select($"id,{TestData.FakeRoleAttributeName}"))
            .Returns(graphServiceUsersCollectionRequestMock.Object);
        graphServiceUsersCollectionRequestMock.Setup(m => m.GetAsync(CancellationToken.None))
            .ReturnsAsync(users);
        var graphServiceUsersCollectionRequestBuilderMock = new Mock<IGraphServiceUsersCollectionRequestBuilder>();
        graphServiceUsersCollectionRequestBuilderMock.Setup(m => m.Request())
            .Returns(graphServiceUsersCollectionRequestMock.Object);
        var graphClientMock = new Mock<GraphServiceClient>(new Mock<IAuthenticationProvider>().Object,
            new Mock<IHttpProvider>().Object);
        graphClientMock.Setup(m => m.Users)
            .Returns(graphServiceUsersCollectionRequestBuilderMock.Object);
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClientMock.Object);

        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new UserRolesGraphService(graphClientFactoryMock.Object, rolesAttributeNameResolver);
        //When
        var result = await sut.Get(CancellationToken.None);
        //Then
        graphClientMock.Verify(m => m.Users);
        graphServiceUsersCollectionRequestMock.Verify(m => m.Select($"id,{TestData.FakeRoleAttributeName}"));
        graphServiceUsersCollectionRequestMock.Verify(m => m.GetAsync(It.IsAny<CancellationToken>()));
        graphServiceUsersCollectionRequestBuilderMock.Verify(m => m.Request());
        result.Should().BeEquivalentTo(new[]
        {
            new
            {
                Id = users[0].Id,
                Roles = Enumerable.Empty<string>()
            },
            new
            {
                Id = users[1].Id,
                Roles = fakeRolesAttribute.Roles
            },
            new
            {
                Id = users[2].Id,
                Roles = fakeRolesAttribute.Roles
            },
            
        }, o => o.ExcludingMissingMembers());
    }
}