using System.Net;
using LeaveSystem.Web.Pages.UsersManagement;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;

namespace LeaveSystem.Web.UnitTests.Pages.UsersManagment;

public class EditUserTest
{
    private HttpClient httpClient = null!;

    private UsersService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenResponseStatusIsSuccessStatusCode_ThenNotThrowAnException()
    {
        //Given
        var user = new UserDto("fakeId123", "George", "fake.george@gmail.com", new[] { "admin", "worker" });
        httpClient = HttpClientMockFactory.CreateWithJsonContent($"api/users/{user.Id}", user, HttpStatusCode.NoContent);
        var sut = GetSut();
        //When
        var act = async () => await sut.Edit(user);
        //Then
        await act.Should().NotThrowAsync<Exception>();
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
        httpClient = HttpClientMockFactory.CreateWithJsonContent($"api/users/{user.Id}", user, statusCode);
        var sut = GetSut();
        //When
        var act = async () => await sut.Edit(user);
        //Then
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
