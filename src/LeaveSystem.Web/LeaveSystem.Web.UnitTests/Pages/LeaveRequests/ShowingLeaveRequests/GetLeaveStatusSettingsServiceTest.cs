using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;

namespace LeaveSystem.Web.UnitTests.Pages.LeaveRequests.ShowingLeaveRequests;

public class GetLeaveStatusSettingsServiceTest
{
    private readonly HttpClient httpClient;
    private IEnumerable<GetLeaveStatusSettingsService.Setting> data;

    public GetLeaveStatusSettingsServiceTest()
    {
        var url = $"api/settings?$filter=Category eq '{SettingCategoryType.LeaveStatus}'";
        data = new[]
        {
            new GetLeaveStatusSettingsService.Setting("1", new GetLeaveStatusSettingsService.SettingValue("blue")),
            new GetLeaveStatusSettingsService.Setting("2", new GetLeaveStatusSettingsService.SettingValue("red")),
            new GetLeaveStatusSettingsService.Setting("3", new GetLeaveStatusSettingsService.SettingValue("green")),
        };
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, data);
    }

    private GetLeaveStatusSettingsService GetSut() => new GetLeaveStatusSettingsService(httpClient);

    [Fact]
    public async Task WhenGetSettings_ThenProvideExpected()
    {
        //Given
        var sut = GetSut();
        //When
        var result = await sut.Get();
        //Then
        result.Should().BeEquivalentTo(data);
    }
}