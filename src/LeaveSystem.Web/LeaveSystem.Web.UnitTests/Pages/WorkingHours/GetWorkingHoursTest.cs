using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using Microsoft.Extensions.Logging;
using FakeWorkingHoursProvider = LeaveSystem.UnitTests.Providers.FakeWorkingHoursProvider;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class GetWorkingHoursTest
{
    [Fact]
    public async Task WhenGetWorkingHours_ThenReturnExceptedDeserializedResult()
    {
        //Given
        var expectedResponse = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToDto().ToPagedListResponse();
        var query = GetWorkingHoursQuery.GetDefault();
        var url = query.CreateQueryString("api/workingHours");
        var httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, expectedResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web), out var mockedHttpValues);
        var toastService = Substitute.For<IToastService>();
        var logger = Substitute.For<ILogger<WorkingHoursService>>();
        var sut = new WorkingHoursService(httpClient, toastService, logger);
        //When
        var result = await sut.GetWorkingHours(query);
        //Then
        result.Should().BeEquivalentTo(expectedResponse);
        mockedHttpValues.ShouldMatchCount();
    }
}
