using System.Globalization;
using System.Text.Json;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using Microsoft.Extensions.Primitives;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class GetWorkingHoursTest
{
    private HttpClient httpClient;

    private WorkingHoursService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenGetWorkingHours_ThenReturnExceptedDeserializedResult()
    {
        //Given
        var expectedResponse = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToDto().ToPagedListResponse();
        var query = GetWorkingHoursQuery.GetDefault();
        var url = query.CreateQueryString("api/workingHours");
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, expectedResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web), out var mockedHttpValues);
        var sut = GetSut();
        //When
        var result = await sut.GetWorkingHours(query);
        //Then
        //Todo: discover why items in result are objects with default properties values
        result.Should().BeEquivalentTo(expectedResponse);
        mockedHttpValues.ShouldMatchCount();
    }
}