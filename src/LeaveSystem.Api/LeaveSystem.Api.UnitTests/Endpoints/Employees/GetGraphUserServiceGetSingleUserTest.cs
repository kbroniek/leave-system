using System.Text.Json;
using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Api.UnitTests.Providers;
using LeaveSystem.Shared;
using Microsoft.Graph;
using NSubstitute;

namespace LeaveSystem.Api.UnitTests.Endpoints.Employees;

public class GetGraphUserServiceGetSingleUserTest
{
    [Fact]
    public async Task WhenUserFound_ThenReturnHimAsFederatedUser()
    {
        //Given
        var graphClientFactoryMock = Substitute.For<IGraphClientFactory>();
        var graphClientMock = Substitute.For<GraphServiceClient>(Substitute.For<IAuthenticationProvider>(), Substitute.For<IHttpProvider>());
        var userRequestBuilderMock = Substitute.For<IUserRequestBuilder>();
        var userRequestMock = Substitute.For<IUserRequest>();
        const string query = $"id,mail,displayName,identities,{TestData.FakeRoleAttributeName}";
        userRequestMock.Select(query).Returns(userRequestMock);
        const string fakeId = "1";
        var user = new User()
        {
            Id = fakeId,
            Mail = "fake.mail@gmail.com",
            DisplayName = "jack.fake",
            AdditionalData = new Dictionary<string, object>()
            {
                {"fakeattr", "fakedata"},
                {TestData.FakeRoleAttributeName, TestData.FakeRolesJson}
            }
        };
        userRequestMock.GetAsync(CancellationToken.None).Returns(user);
        userRequestBuilderMock.Request().Returns(userRequestMock);
        graphClientMock.Users[fakeId].Returns(userRequestBuilderMock);
        graphClientFactoryMock.Create().Returns(graphClientMock);
        var resolver = new RoleAttributeNameResolver(TestData.FakeRoleAttributeName);
        var sut = new GetGraphUserService(graphClientFactoryMock, resolver, FakeDateServiceProvider.GetDateService());
        //When
        var result = await sut.Get(fakeId, CancellationToken.None);
        //Then
        var fakeRolesAttribute = JsonSerializer.Deserialize<RolesResult>(TestData.FakeRolesJson)!;
        result.Should().BeEquivalentTo(new
        {
            user.Id,
            Email = user.Mail,
            Name = user.DisplayName,
            fakeRolesAttribute.Roles
        });
    }
}
