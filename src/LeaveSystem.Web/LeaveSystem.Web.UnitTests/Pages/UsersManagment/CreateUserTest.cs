using System.Net;
using LeaveSystem.Web.Pages.UsersManagement;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;

namespace LeaveSystem.Web.UnitTests.Pages.UsersManagment;

public class CreateUserTest
{
    private HttpClient httpClient = null!;

    private UsersService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenResponseStatusIsSuccessStatusCode_ThenNotThrowAnException()
    {
        //Given
        var user = new UserDto("fakeId123", "George", "fake.george@gmail.com", new[] { "admin", "worker" });
        var content = new StringContent(user.Id ?? string.Empty);
        httpClient = HttpClientMockFactory.CreateWithJsonContent($"api/users", user, HttpStatusCode.Created, content);
        var sut = GetSut();
        //When
        var id = string.Empty;
        var act = async () => id = await sut.Create(user);
        //Then
        await act.Should().NotThrowAsync<Exception>();
        id.Should().Be(user.Id);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task WhenResponseStatusIsNotSuccessStatusCode_ThenThrowAnInvalidOperationException(HttpStatusCode statusCode)
    {
        //Given
        var user = new UserDto("fakeId123", "George", "fake.george@gmail.com", new[] { "admin", "worker" });
        httpClient = HttpClientMockFactory.CreateWithJsonContent($"api/users", user, statusCode);
        var sut = GetSut();
        //When
        var act = async () => await sut.Create(user);
        //Then
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
