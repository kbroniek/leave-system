using System.Net;
using LeaveSystem.Web.Pages.UsersManagement;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;

namespace LeaveSystem.Web.UnitTests.Pages.UsersManagment;

public class DeleteUserTest
{
    private HttpClient httpClient;

    private UsersService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenResponseStatusIsSuccessStatusCode_ThenNotThrowAnException()
    {
        //Given
        var userId = "fakeId123";
        httpClient = HttpClientMockFactory.Create($"api/users/{userId}", HttpStatusCode.Created);
        var sut = GetSut();
        //When
        var act = async () =>
        {
            await sut.Delete(userId);
        };
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
        var userId = "fakeId123";
        httpClient = HttpClientMockFactory.Create($"api/users", statusCode);
        var sut = GetSut();
        //When
        var act = async () =>
        {
            await sut.Delete(userId);
        };
        //Then
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}