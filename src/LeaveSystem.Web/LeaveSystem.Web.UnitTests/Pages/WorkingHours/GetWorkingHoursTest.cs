using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using Microsoft.Extensions.Logging;
using FakeWorkingHoursProvider = LeaveSystem.UnitTests.Providers.FakeWorkingHoursProvider;

namespace LeaveSystem.Web.UnitTests.Pages.WorkingHours;

public class GetWorkingHoursTest
{
    private HttpClient httpClient;
    private IToastService toastService;
    private ILogger<WorkingHoursService> logger;

    private WorkingHoursService GetSut() => new(httpClient, toastService, logger);

    [Fact]
    public async Task WhenGetWorkingHours_ThenReturnExceptedDeserializedResult()
    {
        //Given
        var expectedResponse = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToDto().ToPagedListResponse();
        var query = GetWorkingHoursQuery.GetDefault();
        var url = query.CreateQueryString("api/workingHours");
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, expectedResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web), out var mockedHttpValues);
        toastService = Substitute.For<IToastService>();
        logger = Substitute.For<ILogger<WorkingHoursService>>();
        var sut = GetSut();
        //When
        var result = await sut.GetWorkingHours(query);
        //Then
        result.Should().BeEquivalentTo(expectedResponse);
        mockedHttpValues.ShouldMatchCount();
    }
}