using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentAssertions.Json;
using LeaveSystem.Api.Endpoints.Roles;
using LeaveSystem.Api.GraphApi;
using Microsoft.Graph;
using Moq;
using Newtonsoft.Json.Linq;

namespace LeaveSystem.Api.UnitTests.Endpoints.Roles;

public class UserRolesGraphServiceUpdateTest
{
    [Fact]
    public async Task WhenThereIsUserWithGivenId_ThenUpdateUser()
    {
        //Given
        var clientFactoryMock = new Mock<IGraphClientFactory>();
        var clientMock = new Mock<GraphServiceClient>(new Mock<IAuthenticationProvider>().Object,
            new Mock<IHttpProvider>().Object);
        clientFactoryMock.Setup(m => m.Create())
            .Returns(clientMock.Object);
        const string fakeId = "1";
        var graphServiceUsersCollectionRequestBuilderMock = new Mock<IGraphServiceUsersCollectionRequestBuilder>();
        var graphUserRequestMock = new Mock<IUserRequest>();
        var updatedUser = new User();
        graphUserRequestMock.Setup(m => m.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u,c) => { updatedUser = u; });
        var graphUserRequestBuilderMock = new Mock<IUserRequestBuilder>();
        graphUserRequestBuilderMock.Setup(m => m.Request())
            .Returns(graphUserRequestMock.Object);
        graphServiceUsersCollectionRequestBuilderMock.Setup(m => m[fakeId])
            .Returns(graphUserRequestBuilderMock.Object);
        clientMock.Setup(m => m.Users)
            .Returns(graphServiceUsersCollectionRequestBuilderMock.Object);
        var rolesAttributeNameResolver = new RoleAttributeNameResolver("fakeRolesAttr");
        var sut = new UserRolesGraphService(clientFactoryMock.Object, rolesAttributeNameResolver);
        var roles = new[] { "role1", "role2", "role3" };
        //When
        await sut.Update(fakeId, roles, CancellationToken.None);
        //Then
        clientMock.Verify(m => m.Users, Times.Once());
        graphServiceUsersCollectionRequestBuilderMock.Verify(m => m[fakeId], Times.Once());
        graphUserRequestBuilderMock.Verify(m => m.Request(), Times.Once());
        graphUserRequestMock.Verify(m => m.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()
        ), Times.Once());
        updatedUser.Should().BeEquivalentTo(new User
            {
                AdditionalData = new Dictionary<string, object>
                {
                    {
                        rolesAttributeNameResolver.RoleAttributeName,
                        """{"Roles":["role1","role2","role3"]}"""
                    }
                }
            },
            o => o
                .Using<Dictionary<string,object>>(CompareAdditionalData)
                .When(i => i.Path.EndsWith(nameof(User.AdditionalData)))
        );
    }

    private static void CompareAdditionalData(IAssertionContext<Dictionary<string,object>> context)
    {
        var subjectAdditionalDataKvp = context.Subject;
        var expectationAdditionalDataKvp = context.Expectation;
        var keys = subjectAdditionalDataKvp.Keys;
        foreach (var key in keys)
        {
            var subjectAdditionalDataJson = subjectAdditionalDataKvp[key].ToString() ?? "{}";
            expectationAdditionalDataKvp.TryGetValue(key, out var expectationAdditionalData).Should().BeTrue();
            var expectationAdditionalDataJson = expectationAdditionalData?.ToString() ?? "{}";
            JToken.Parse(subjectAdditionalDataJson).Should()
                .BeEquivalentTo(JToken.Parse(expectationAdditionalDataJson));
        }
    }
}