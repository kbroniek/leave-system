using LeaveSystem.Web.Pages.UsersManagement;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;

namespace LeaveSystem.Web.UnitTests.Pages.UsersManagment;

public class GetUsersTest
{
    private HttpClient httpClient;

    private UsersService GetSut() => new(httpClient);
    
    [Fact]
    public async Task WhenGettingUsers_ThenReturnDeserializedNotNullUsers()
    {
        //Given
        var users = FakeUserDtoProvider.GetAll();
        httpClient = HttpClientMockFactory.CreateWithJsonResponse("api/users", new UsersDto(users.Append(null)));
        var sut = GetSut();
        //When
        var result = await sut.Get();
        //Then
        result.Should().BeEquivalentTo(users);
    }
    
    [Theory]
    [MemberData(nameof(Get_WhenUsersDtoItemsAreNull_ThenReturnEmptyCollection_TestData))]
    public async Task WhenUsersDtoItemsAreNull_ThenReturnEmptyCollection(UsersDto users)
    {
        //Given
        httpClient = HttpClientMockFactory.CreateWithJsonResponse("api/users", users);
        var sut = GetSut();
        //When
        var result = await sut.Get();
        //Then
        result.Should().BeEmpty();
    }

    public static IEnumerable<object[]> Get_WhenUsersDtoItemsAreNull_ThenReturnEmptyCollection_TestData()
    {
        yield return new object[] { null };
    }
}