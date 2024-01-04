using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Api.UnitTests.Providers;
using LeaveSystem.Shared;
using Microsoft.Graph;
using Moq;
using System.Text.Json;

namespace LeaveSystem.Api.UnitTests.Endpoints.Employees;

public class GetGraphUserServiceGetTest
{
    [Fact]
    public async Task WhenGetting_ThenReturnUsers()
    {
        //Given
        var fakeRolesAttribute = JsonSerializer.Deserialize<RolesResult>(TestData.FakeRolesJson);
        var users = GraphServiceUsersCollectionPageProvider.Get(TestData.FakeRoleAttributeName, TestData.FakeRolesJson);
        var graphClientFactoryMock = new Mock<IGraphClientFactory>();
        var graphServiceUsersCollectionRequestMock = new Mock<IGraphServiceUsersCollectionRequest>();
        const string query = $"id,mail,displayName,identities,{TestData.FakeRoleAttributeName}";
        graphServiceUsersCollectionRequestMock.Setup(m => m.Select(query))
            .Returns(graphServiceUsersCollectionRequestMock.Object);
        graphServiceUsersCollectionRequestMock.Setup(m => m.GetAsync(CancellationToken.None))
            .ReturnsAsync(users);
        var graphServiceUsersCollectionRequestBuilderMock = new Mock<IGraphServiceUsersCollectionRequestBuilder>();
        graphServiceUsersCollectionRequestBuilderMock.Setup(m => m.Request())
            .Returns(graphServiceUsersCollectionRequestMock.Object);
        var graphClientMock = new Mock<GraphServiceClient>(new Mock<IAuthenticationProvider>().Object, new Mock<IHttpProvider>().Object);
        graphClientMock.Setup(m => m.Users)
            .Returns(graphServiceUsersCollectionRequestBuilderMock.Object);
        graphClientFactoryMock.Setup(x => x.Create())
            .Returns(graphClientMock.Object);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new GetGraphUserService(graphClientFactoryMock.Object, rolesAttributeNameResolver, FakeDateServiceProvider.GetDateService());
        //When
        var federatedUsers = await sut.Get(CancellationToken.None);
        //Then
        graphClientMock.Verify(m => m.Users);
        graphServiceUsersCollectionRequestMock.Verify(m => m.Select(query));
        graphServiceUsersCollectionRequestMock.Verify(m => m.GetAsync(It.IsAny<CancellationToken>()));
        graphServiceUsersCollectionRequestBuilderMock.Verify(m => m.Request());
        federatedUsers.Should().BeEquivalentTo(new[]
        {
            new
            {
                Id = users[0].Id,
                Email = users[0].Mail,
                Roles = Enumerable.Empty<string>(),
                Name = users[0].DisplayName
            },
            new
            {
                Id = users[1].Id,
                Email = users[1].Mail,
                Roles = fakeRolesAttribute.Roles,
                Name = users[1].DisplayName
            },
            new
            {
                Id = users[2].Id,
                Email = users[2].Mail,
                Roles = fakeRolesAttribute.Roles,
                Name = users[2].DisplayName
            },
        }, o => o.ExcludingMissingMembers());
    }
}